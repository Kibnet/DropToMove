using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;

namespace DropToMove
{
	/// <summary>
	///     Логика взаимодействия для DropWindow.xaml
	/// </summary>
	public partial class DropWindow : Window
	{
		public DropWindow()
		{
			InitializeComponent();
		}

		private void UIElement_OnDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
				ContextViewModel.Action(files);
			}
		}


		private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var dlg = new FolderBrowserDialog();
			if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				ContextViewModel.Destination = dlg.SelectedPath;
			}
		}
	}
}