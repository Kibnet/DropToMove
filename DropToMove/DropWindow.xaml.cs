using System.IO;
using System.Text;
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
				if (e.KeyStates == DragDropKeyStates.ShiftKey)
				{
					foreach (var file in files)
					{
						var dir = new DirectoryInfo(file);
						if (dir.Exists)
						{
							ContextViewModel.Destination = dir.FullName;
							break;
						}
						var inf = new FileInfo(file);
						if (inf.Exists && inf.Directory != null)
						{
							ContextViewModel.Destination = inf.Directory.FullName;
							break;
						}
					}
				}
				else
				{
					ContextViewModel.Action(files);
				}
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