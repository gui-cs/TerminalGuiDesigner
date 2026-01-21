

using System.Runtime.CompilerServices;
using System.Threading;

namespace UnitTests;


/// <summary>
///     Provides methods to create and manage a fake application for testing purposes.
/// </summary>
public class FakeApplicationFactory
{
    /// <summary>
    ///     Creates an initialized fake application which will be cleaned up when result object
    ///     is disposed.
    /// </summary>
    /// <returns></returns>
    public IDisposable SetupFakeApplication(out IApplication application)
    {
        CancellationTokenSource hardStopTokenSource = new CancellationTokenSource();
        AnsiInput ansiInput = new AnsiInput();
        ansiInput.ExternalCancellationTokenSource = hardStopTokenSource;
        AnsiOutput output = new();
        output.SetSize(80, 25);

        SizeMonitorImpl sizeMonitor = new(output);

        ApplicationImpl impl = new(new AnsiComponentFactory(ansiInput, output, sizeMonitor));
        ApplicationImpl.SetInstance(impl);

        // Initialize with a ANSI driver
        impl.Init(DriverRegistry.Names.ANSI);
        impl.Driver!.Clipboard = new FakeClipboard();

        application = impl;

        return new FakeApplicationLifecycle(impl, hardStopTokenSource);
    }
}
#nullable enable

/// <summary>
///     Implements a fake application lifecycle for testing purposes. Cleans up the application on dispose by cancelling
///     the provided <see cref="CancellationTokenSource"/> and shutting down the application.
/// </summary>
/// <param name="hardStop"></param>
internal class FakeApplicationLifecycle(IApplication? app, CancellationTokenSource? hardStop) : IDisposable
{
    /// <inheritdoc/>
    public void Dispose()
    {
        hardStop?.Cancel();

        app?.TopRunnableView?.Dispose();
        app?.Dispose();
    }
}


[RequiresThread]
public class Tests
{
    private MouseManager mm;

    /// <summary>
    /// Mock IApplication instance for use in tests. Created fresh for each test.
    /// </summary>
    protected IApplication App { get; private set; } = null!;

    [SetUp]
    public virtual void SetUp()
    {
        var appFactory = new FakeApplicationFactory();
        appFactory.SetupFakeApplication(out var a);
        App = a;

        OperationManager.Instance.ClearUndoRedo();

        mm = new MouseManager(App);
    }

    [TearDown]
    public virtual void TearDown()
    {
        SelectionManager.Instance.LockSelection = false;
        SelectionManager.Instance.Clear();
    }

    protected Design Get10By10View()
    {
        var v = new View()
        {
            Width = 10,
            Height = 10,
            CanFocus = true,
        };
        var d = new Design(App, new SourceCodeFile(new FileInfo("TenByTen.cs")), Design.RootDesignName, v);
        v.Data = d;

        v.BeginInit();
        v.EndInit();

        App.TopRunnableView.Add(v);
        App.TopRunnableView.LayoutSubViews();

        return d;
    }

    protected Design Get100By100<T>([CallerMemberName] string? caller = null)
    {
        // start with blank slate
        OperationManager.Instance.ClearUndoRedo();

        var viewToCode = new ViewToCode(App);

        var file = new FileInfo($"{caller}.cs");
        var rootDesign = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Window));
        rootDesign.View.X = 100;
        rootDesign.View.Y = 100;

        return rootDesign;
    }

    /// <summary>
    /// Creates a new instance of <typeparamref name="T2"/> using <see cref="ViewFactory"/>.  Then calls the
    /// provided <paramref name="adjust"/> action before writing out and reading back the code.  Returns
    /// the read back in instance of your <typeparamref name="T2"/> so you can compare that it matches expectations
    /// (i.e. nothing was lost during serialization/deserialization).
    /// </summary>
    /// <typeparam name="T1">Root designer View type to create (e.g. <see cref="Window"/>)</typeparam>
    /// <typeparam name="T2">Type of subview to create (e.g. <see cref="Label"/>)</typeparam>
    /// <param name="app">The IApplication instance to use.</param>
    /// <param name="adjust">Mutator for making pre save changes you want to conform can be read in properly</param>
    /// <param name="viewOut">The view created and passed to <paramref name="adjust"/></param>
    /// <param name="caller"></param>
    /// <returns>The read in object state after round trip (generate code file then read that code back in)</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="caller"/> is <see langword="null" />, empty, or whitespace</exception>
    protected T2 RoundTrip<T1, T2>(Action<Design, T2> adjust, out T2 viewOut, [CallerMemberName] string? caller = null)
        where T1 : View, new()
        where T2 : View, new()
    {
        if ( string.IsNullOrWhiteSpace( caller ) )
        {
            throw new ArgumentNullException( nameof( caller ), "Cannot create an item with no name." );
        }

        const string fieldName = "myViewOut";

        var viewToCode = new ViewToCode(App);

        var file = new FileInfo(caller + ".cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(T1));

        viewOut = (T2)ViewFactory.Create(typeof(T2));

        OperationManager.Instance.Do(new AddViewOperation(App, viewOut, designOut, fieldName));
        adjust((Design)viewOut.Data, viewOut);

        viewToCode.GenerateDesignerCs(designOut, typeof(T1));

        var codeToView = new CodeToView(App, designOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        return designBackIn.View
                           .GetActualSubviews( )
                           .OfType<T2>( )
                           .Single( static v => v.Data is Design { FieldName: fieldName } );
    }
    /// <summary>
    /// Performs a mouse drag from the first coordinates to the second (in screen space)
    /// </summary>
    /// <param name="app">The IApplication instance to use.</param>
    /// <param name="root">The root Design.  Make sure you have added it to <see cref="App.TopRunnableView"/> and run <see cref="View.LayoutSubViews"/></param>
    /// <param name="x1">X coordinate to start drag at</param>
    /// <param name="y1">Y coordinate to start drag at</param>
    /// <param name="x2">X coordinate to end drag at</param>
    /// <param name="y2">Y coordinate to end drag at</param>
    protected void MouseDrag(Design root, int x1, int y1, int x2, int y2)
    {
        mm.HandleMouse(
            new Mouse
            {
                Position = new Point(x1, y1),
                Flags = MouseFlags.LeftButtonPressed,
            }, root);

        // press down at 0,0 of the label
        mm.HandleMouse(
            new Mouse
            {
                Position = new Point(x2, y2),
                Flags = MouseFlags.LeftButtonPressed,
            }, root);


        // release in parent
        mm.HandleMouse(
            new Mouse
            {
                Position = new Point(x2,y2),
                Flags = MouseFlags.LeftButtonReleased,
            }, root);
    }

    public static Type PickFirstTTypeForGenerics(Type type)
    {
        if (type.IsGenericTypeDefinition)
        {
            var tType = TTypes.GetSupportedTTypesForGenericViewOfType(type).First();
            return type.MakeGenericType(tType);
        }

        return type;
    }
}