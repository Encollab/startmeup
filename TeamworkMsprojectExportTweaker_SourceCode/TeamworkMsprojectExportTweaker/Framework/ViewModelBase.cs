using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;

namespace TeamworkMsprojectExportTweaker
{
	public abstract class ViewModelBase : INotifyPropertyChanged
	{
		protected readonly static TaskFactory uiTaskFactory;
		private static bool? _isInDesignMode;

		static ViewModelBase()
		{
			if (IsInDesignMode)
			{
				//var exeName = typeof(ViewModelBase).Name;
				//var vshost = Process.GetProcesses().First(_process => _process.ProcessName.Contains(exeName)).Modules[0].FileName;
				//var solutionDir = new FileInfo(vshost).Directory.Parent.Parent.Parent;
			}
			else
			{
				var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
				uiTaskFactory = new TaskFactory(uiScheduler);
			}
		}

		public static bool IsInDesignMode
		{
			get
			{
				if (!_isInDesignMode.HasValue)
				{
					_isInDesignMode = (bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue;
				}

				return _isInDesignMode.GetValueOrDefault();
			}
		}

		#region INotifyPropertyChanged Members

		public virtual event PropertyChangedEventHandler PropertyChanged;

		[DebuggerStepThrough]
		protected void OnPropertyChanged(params string[] propertyNames)
		{
			foreach (string name in propertyNames)
			{
				PropertyChangedEventHandler handler = this.PropertyChanged;
				if (handler != null)
				{
					handler(this, new PropertyChangedEventArgs(name));
				}
			}
		}

		[DebuggerStepThrough]
		protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
		{
			var memberExpr = propertyExpression.Body as MemberExpression;
			if (memberExpr == null)
			{
				throw new ArgumentException("The expression is not a member access expression.", "propertyExpression");
			}
			string memberName = memberExpr.Member.Name;
			OnPropertyChanged(memberName);
		}

		#endregion INotifyPropertyChanged Members
	}
}