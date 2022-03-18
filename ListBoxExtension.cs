using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvModsExporter
{
    public static class ListBoxExtension
    {
        public static void MoveSelectedItemUp(this ListBox listBox)
        {
            _MoveSelectedItem(listBox, -1);
        }

        public static void MoveSelectedItemDown(this ListBox listBox)
        {
            _MoveSelectedItem(listBox, 1);
        }

        static void _MoveSelectedItem(ListBox listBox, int direction)
        {
            // Checking selected item
            if (listBox.SelectedItem == null || listBox.SelectedIndex < 0)
                return; // No selected item - nothing to do

            // Calculate new index using move direction
            int newIndex = listBox.SelectedIndex + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= listBox.Items.Count)
                return; // Index out of range - nothing to do

            object selected = listBox.SelectedItem;

            // Save checked state if it is applicable
            var checkedListBox = listBox as CheckedListBox;
            var checkState = CheckState.Unchecked;
            if (checkedListBox != null)
                checkState = checkedListBox.GetItemCheckState(checkedListBox.SelectedIndex);

            // Removing removable element
            listBox.Items.Remove(selected);
            // Insert it in new position
            listBox.Items.Insert(newIndex, selected);
            // Restore selection
            listBox.SetSelected(newIndex, true);

            // Restore checked state if it is applicable
            if (checkedListBox != null)
                checkedListBox.SetItemCheckState(newIndex, checkState);
        }
    }

    public static class ListViewExtension
    {
        public static void MoveSelectedItemUp(this ListView listView)
        {
            _MoveSelectedItem(listView, -1);
        }

        public static void MoveSelectedItemDown(this ListView listView)
        {
            _MoveSelectedItem(listView, 1);
        }

        static void _MoveSelectedItem(ListView listView, int direction)
        {
            // Checking selected item
            if (listView.SelectedItems == null || listView.SelectedItems.Count != 1)
                return; // No selected item - nothing to do

            // Calculate new index using move direction
            int newIndex = listView.SelectedIndices[0] + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= listView.Items.Count)
                return; // Index out of range - nothing to do

            ListViewItem selected = listView.SelectedItems[0];

            // Save checked state if it is applicable
            var checkedListView = listView as ListView;
            var checkState = CheckState.Unchecked;
            if (checkedListView != null)
                checkState = checkedListView.SelectedItems[0].Checked ? CheckState.Checked : CheckState.Unchecked;

            // Removing removable element
            listView.Items.Remove(selected);
            // Insert it in new position
            listView.Items.Insert(newIndex, selected);
            // Restore selection
            listView.Items[newIndex].Selected = true;

            // Restore checked state if it is applicable
            if (checkedListView != null)
                checkedListView.Items[newIndex].Checked = checkState == CheckState.Checked;
        }

        public static void DeselectAll(this ListView listView)
        {
            for (int i = 0; i < listView.Items.Count; i++)
            {
                listView.Items[i].Selected = false;
            }
        }
    }
}
