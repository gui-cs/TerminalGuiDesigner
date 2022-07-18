using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using static Terminal.Gui.TabView;

namespace TerminalGuiDesigner
{
    public static class TabViewExtensions
    {
        /// <summary>
        /// Inserts or moves <paramref name="tab"/> to <paramref name="atIndex"/> and makes it
        /// the selected tab
        /// </summary>
        /// <param name="tv"></param>
        /// <param name="atIndex"></param>
        /// <param name="tab"></param>
        /// <returns></returns>
        public static bool InsertTab(this TabView tv, int atIndex, Tab tab)
        {
            var list = tv.Tabs.ToList();

            var newIndex = Math.Max(0, Math.Min(list.Count - 1, atIndex));

            list.Remove(tab);
            list.Insert(newIndex, tab);

            var origTabs = tv.Tabs;

            // View.Tabs is readonly so we have to
            // remove all the tabs
            foreach (var t in origTabs.ToArray())
            {
                tv.RemoveTab(t);
            }

            // then add them back in again in the new order
            for (int i = 0; i < list.Count; i++)
            {
                var t = list[i];

                // put all the tabs back in again and select
                // the tab in it's new position
                tv.AddTab(t, i == newIndex);
            }

            return true;
        }
    }
}
