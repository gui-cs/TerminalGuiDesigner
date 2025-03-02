using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Terminal.Gui;

namespace TerminalGuiDesigner.UI.Windows;

/// <summary>
/// Modal list box with search.  Displays a collection of objects of Type
/// <typeparamref name="T"/> and gets the user to pick one.
/// </summary>
/// <typeparam name="T">Class Type for the objects the user is to pick from.</typeparam>
public class BigListBox<T>
{
    private static readonly string[ ] ErrorStringArray = new[] { "Error" };
    private readonly object taskCancellationLock = new();
    private readonly ConcurrentBag<CancellationTokenSource> cancelFiltering = new();
    private readonly Window win;
    private readonly ListView listView;
    private readonly bool addNull;
    private readonly TextField? searchBox;
    private readonly object callback;

    /// <summary>
    /// If the public constructor was used then this is the fixed list we were initialized with.
    /// </summary>
    private readonly IList<T> publicCollection;

    private IList<ListViewObject<T>> collection;


    /// <summary>
    /// Ongoing filtering of a large collection should be cancelled when the user changes the filter even if it is not completed yet.
    /// </summary>
    private bool changes;

    private DateTime lastKeypress = DateTime.Now;

    private bool okClicked = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="BigListBox{T}"/> class.
    /// </summary>
    /// <param name="prompt">User message indicating what they are being asked to select.</param>
    /// <param name="okText">The confirmation text on the 'ok' button.</param>
    /// <param name="addSearch">True to add search text box.</param>
    /// <param name="collection">Things to make user pick from.</param>
    /// <param name="displayMember">What to display in the list box (defaults to <see cref="object.ToString"/>.</param>
    /// <param name="addNull">Creates a selection option "Null" that returns a null selection.</param>
    /// <param name="currentSelection">The optional existing value, if present it should be selected in the list.</param>
    public BigListBox(
        string prompt,
        string okText,
        in bool addSearch,
        IList<T> collection,
        Func<T?, string> displayMember,
        bool addNull,
        T? currentSelection)
    {
        this.AspectGetter = displayMember ?? (arg => arg?.ToString() ?? string.Empty);

        this.publicCollection = collection ?? throw new ArgumentNullException( nameof( collection ) );
        this.addNull = addNull;

        this.win = new Window()
        {
            X = 0,
            Y = 0,
            Title = prompt,

            // By using Dim.Fill(), it will automatically resize without manual intervention
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Modal = true,
        };

        this.listView = new ListView()
        {
            X = 0,
            Y = 0,
            Height = Dim.Fill(2),
            Width = Dim.Fill(2),
            SelectedItem = 0
        };
        listView.SetSource(new ObservableCollection<string>(ErrorStringArray));

        this.listView.KeyDown += this.ListView_KeyPress;

        this.listView.MouseClick += this.ListView_MouseClick;
        
        this.collection = this.BuildList(this.GetInitialSource()).ToList();

        this.listView.SetSource(
            new ObservableCollection<ListViewObject<T>>(this.collection.ToArray())
            );
        this.win.Add(this.listView);

        var btnOk = new Button()
        {
            Text = okText,
            IsDefault = true,
            Y = Pos.Bottom(this.listView),
        };
        btnOk.Accepting += (s, e) =>
        {
            e.Cancel = true;
            this.Accept();
        };

        var btnCancel = new Button()
        {
            Text = "Cancel",
            Y = Pos.Bottom(this.listView),
        };
        btnCancel.Accepting += (s, e) =>
        {
            e.Cancel = true;
            Application.RequestStop();
        };

        if (addSearch)
        {
            var searchLabel = new Label()
            {
                Text = "Search:",
                X = 0,
                Y = Pos.Bottom(this.listView),
            };

            this.win.Add(searchLabel);

            this.searchBox = new TextField()
            {
                Text = string.Empty,
                X = Pos.Right(searchLabel),
                Y = Pos.Bottom(this.listView),
                Width = 30,
            };

            btnOk.X = 38;
            btnCancel.X = Pos.Right(btnOk) + 2;

            this.win.Add(this.searchBox);
            this.searchBox.SetFocus();

            this.searchBox.TextChanged += (s, e) =>
            {
                // Don't update the UI while user is hammering away on the keyboard
                this.lastKeypress = DateTime.Now;
                this.RestartFiltering();
            };
        }
        else
        {
            btnOk.X = Pos.Center() - 5;
            btnCancel.X = Pos.Center() + 5;
        }

        this.win.Add(btnOk);
        this.win.Add(btnCancel);

        if (currentSelection != null)
        {
            this.SetCurrentSelection(currentSelection);
        }
        else
        {
            this.listView.SelectedItem = 0;
        }

        this.callback = Application.AddTimeout(TimeSpan.FromMilliseconds(100), this.Timer);

        this.listView.FocusDeepest(NavigationDirection.Forward,TabBehavior.TabStop);
    }


    /// <summary>
    /// Gets the value the user chose upon exiting.
    /// </summary>
    public T? Selected { get; private set; }

    /// <summary>
    /// Gets or Sets the delegate that controls what is rendered for each object.
    /// </summary>
    public Func<T?, string> AspectGetter { get; set; }

    /// <summary>
    /// Runs the dialog as modal blocking and returns true if a selection was made.
    /// </summary>
    /// <returns>True if selection was made (see <see cref="Selected"/>) or false if user cancelled the dialog.</returns>
    public bool ShowDialog()
    {
        Application.Run(this.win);

        Application.RemoveTimeout(this.callback);

        return this.okClicked;
    }

    private void Accept()
    {
        var selected = this.listView.SelectedItem;
        if (selected < 0 || selected >= this.collection.Count)
        {
            return;
        }

        this.okClicked = true;
        Application.RequestStop();
        this.Selected = this.collection[selected].Object;
    }

    private void ListView_MouseClick(object? sender, MouseEventArgs obj)
    {
        if (obj.Flags.HasFlag(MouseFlags.Button1DoubleClicked))
        {
            obj.Handled = true;
            this.Accept();
        }
    }

    private void ListView_KeyPress(object? sender, Key key)
    {
        // if user types in some text change the focus to the text box to enable searching
        var c = (char)key;

        // backspace or letter/numbers
        if (key == Key.Backspace || char.IsLetterOrDigit(c))
        {
            this.searchBox?.FocusDeepest(NavigationDirection.Forward, TabBehavior.TabStop);
            this.searchBox?.NewKeyDownEvent(key);
            key.Handled = true;
        }
    }

    private bool Timer()
    {
        if (this.changes && DateTime.Now.Subtract(this.lastKeypress) > TimeSpan.FromMilliseconds(100))
        {
            lock (this.taskCancellationLock)
            {
                var oldSelected = this.listView.SelectedItem;
                this.listView.SetSource(
                    new ObservableCollection<T>(
                        this.collection.Select(l=>l.Object)
                        ));

                if (oldSelected < this.collection.Count)
                {
                    this.listView.SelectedItem = oldSelected;
                }

                this.changes = false;
                return true;
            }
        }

        return true;
    }

    private void RestartFiltering()
    {
        this.RestartFiltering(this.searchBox?.Text.ToString());
    }

    private void RestartFiltering(string? searchTerm)
    {
        lock (this.taskCancellationLock)
        {
            // cancel any previous searches
            foreach (var c in this.cancelFiltering)
            {
                c.Cancel();
            }

            this.cancelFiltering.Clear();
        }

        var cts = new CancellationTokenSource();
        this.cancelFiltering.Add(cts);

        Task.Run(() =>
        {
            var result = this.BuildList(this.GetListAfterSearch(searchTerm, cts.Token));

            lock (this.taskCancellationLock)
            {
                this.collection = result;
                this.changes = true;
            }
        });
    }

    private IList<ListViewObject<T>> BuildList(IList<T> listOfT)
    {
        var toReturn = listOfT.Select(o => new ListViewObject<T>(o, this.AspectGetter)).ToList();

        if (this.addNull)
        {
            toReturn.Add(new ListViewObject<T>(default, (o) => "Null"));
        }

        return toReturn;
    }

    private IList<T> GetListAfterSearch(string? searchString, CancellationToken token)
    {
        if (this.publicCollection == null)
        {
            throw new InvalidOperationException("When using the private constructor derived classes must override this method ");
        }

        if (string.IsNullOrEmpty(searchString))
        {
            return this.publicCollection.ToList();
        }

        // stop the Contains searching when the user cancels the search
        return this.publicCollection.Where(o => !token.IsCancellationRequested &&
            this.AspectGetter(o).Contains(searchString, StringComparison.CurrentCultureIgnoreCase)).ToList();
    }

    private IList<T> GetInitialSource()
    {
        if (this.publicCollection == null)
        {
            throw new InvalidOperationException("When using the private constructor derived classes must override this method ");
        }

        return this.publicCollection;
    }

    private void SetCurrentSelection(T? currentSelection)
    {
        if (currentSelection == null)
        {
            return;
        }

        if (currentSelection is ColorScheme colorScheme)
        {
            var colorSchemeName = ColorSchemeManager.Instance.GetNameForColorScheme(colorScheme);
            var currentSelectionInCollection = this.collection.FirstOrDefault(o => o.Object != null && o.Object is NamedColorScheme scheme && scheme.Name == colorSchemeName);

            if (currentSelectionInCollection != null)
            {
                this.listView.SelectedItem = this.collection.IndexOf(currentSelectionInCollection);
            }
        }
        else
        {
            var currentSelectionInCollection = this.collection.FirstOrDefault(o => o.Object != null && object.Equals(o.Object, currentSelection));

            if (currentSelectionInCollection != null)
            {
                this.listView.SelectedItem = this.collection.IndexOf(currentSelectionInCollection);
            }
        }
    }

    private class ListViewObject<T2>
        where T2 : T
    {
        private readonly Func<T2?, string> displayFunc;

        public ListViewObject(T2? o, Func<T2?, string> displayFunc)
        {
            this.displayFunc = displayFunc;
            this.Object = o;
        }

        public T2? Object { get; }

        public override string ToString()
        {
            return this.displayFunc(this.Object);
        }

        public override int GetHashCode()
        {
            if (this.Object == null)
            {
                return 0;
            }

            return this.Object.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is ListViewObject<T2> other)
            {
                return this.Object?.Equals(other.Object) ?? false;
            }

            return false;
        }
    }
}