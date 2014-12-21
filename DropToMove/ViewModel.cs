using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DropToMove.Annotations;

namespace DropToMove
{
	public class ViewModel : INotifyPropertyChanged
	{
		private readonly BackgroundWorker worker;
		private long _counter;
		private string _destination;
		private FileOperation _operation;
		private string _status;
		private long _tryCounter;
		private bool _topmost;

		public ViewModel()
		{
			Operation = FileOperation.Перемещать;
			Status = "Переместите файлы и папки на это окно для немедленной обработки";
			//"Drag and drop files and folders in this window for immediate processing";
			worker = new BackgroundWorker {WorkerReportsProgress = true};
			worker.ProgressChanged += (sender, args) => { Status = args.UserState.ToString(); };
			worker.RunWorkerCompleted += (sender, args) =>
			{
				OnPropertyChanged("IsWorking");
				OnPropertyChanged("NotWorking");
			};
			worker.DoWork += (sender, args) => RecursiveAction(args.Argument as string[]);
		}

		public FileOperation[] AllOperations
		{
			get { return new[] {FileOperation.Копировать, FileOperation.Перемещать, FileOperation.Удалять}; }
		}

		public FileOperation Operation
		{
			get { return _operation; }
			set
			{
				if (value == _operation) return;
				_operation = value;
				OnPropertyChanged("Operation");
			}
		}

		public string Destination
		{
			get { return _destination; }
			set
			{
				if (value == _destination) return;
				_destination = value;
				OnPropertyChanged("Destination");
			}
		}

		public string Status
		{
			get { return _status; }
			set
			{
				if (value == _status) return;
				_status = value;
				OnPropertyChanged("Status");
			}
		}

		public bool IsWorking
		{
			get { return worker != null && worker.IsBusy; }
		}

		public bool NotWorking
		{ get { return !IsWorking; } }

		public bool Topmost
		{
			get { return _topmost; }
			set
			{
				if (value.Equals(_topmost)) return;
				_topmost = value;
				OnPropertyChanged("Topmost");
			}
		}

		public long Counter
		{
			get { return _counter; }
			set
			{
				if (value == _counter) return;
				_counter = value;
				OnPropertyChanged("Counter");
			}
		}

		public long TryCounter
		{
			get { return _tryCounter; }
			set
			{
				if (value == _tryCounter) return;
				_tryCounter = value;
				OnPropertyChanged("TryCounter");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void Action(string[] entities)
		{
			Counter = 0;
			TryCounter = 0;
			worker.RunWorkerAsync(entities);
			OnPropertyChanged("IsWorking");
			OnPropertyChanged("NotWorking");
		}

		public void RecursiveAction(IEnumerable<string> infos)
		{
			IEnumerable<FileSystemInfo> list = infos.Select(entity =>
			{
				var info = new FileInfo(entity);
				if (info.Exists)
				{
					sourceInfo = info.Directory;
					return info as FileSystemInfo;
				}
				var dir = new DirectoryInfo(entity);
				if (dir.Exists)
				{
					sourceInfo = dir.Parent;
					return dir as FileSystemInfo;
				}
				return null;
			});
			RecursiveAction(list);
		}

		public void RecursiveAction(IEnumerable<FileSystemInfo> infos)
		{
			foreach (FileSystemInfo info in infos)
			{
				try
				{
					if (info is FileInfo)
						DoFile(info as FileInfo);
					else if (info is DirectoryInfo)
					{
						RecursiveAction((info as DirectoryInfo).EnumerateFileSystemInfos());
						(info as DirectoryInfo).Delete();
					}
				}
				catch (Exception e)
				{
					Catch(e);
				}
			}
		}

		private DirectoryInfo sourceInfo;
		private bool _overwrite;

		private void DoFile(FileInfo file)
		{
			if (!file.Exists) return;
			TryCounter++;
			switch (Operation)
			{
				case FileOperation.Удалять:
				{
					file.Delete();
					Status = "Удалён " + file.FullName;
					break;
				}
				case FileOperation.Копировать:
				{
					var newpath = GetNewPath(file);
					file.CopyTo(newpath.FullName, Overwrite);
					Status = "Скопирован " + file.FullName;
					break;
				}
				case FileOperation.Перемещать:
				{
					var newpath = GetNewPath(file);
					file.MoveTo(newpath.FullName);
					Status = "Перемещён " + file.FullName;
					break;
				}
			}
			Counter++;
		}

		private FileInfo GetNewPath(FileInfo file)
		{
			if (sourceInfo == null)
			{
				throw new Exception("Не найден источник файла");
			}
			var dest = new DirectoryInfo(Destination);
			var newpath = new FileInfo(file.FullName.Replace(sourceInfo.FullName, dest.FullName));
			if (newpath.Directory != null && !newpath.Directory.Exists)
				newpath.Directory.Create();
			return newpath;
		}

		public bool Overwrite
		{
			get { return _overwrite; }
			set
			{
				if (value.Equals(_overwrite)) return;
				_overwrite = value;
				OnPropertyChanged("Overwrite");
			}
		}

		private void Catch(Exception e)
		{
			Status = e.Message;
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}