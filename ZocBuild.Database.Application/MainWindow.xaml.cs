﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Data.SqlClient;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZocBuild.Database.Application.Controls;
using ZocBuild.Database.Application.Converters;
using ZocBuild.Database.Application.Settings;
using ZocBuild.Database.Application.ViewModels;
using ZocBuild.Database.ScriptRepositories;
using ZocBuild.Database.SqlParser;
using ZocBuild.Database.Logging;

namespace ZocBuild.Database.Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [Import]
        private IParser sqlParser;
        private readonly FileInfo pathToGit;

        public MainWindow()
        {
            InitializeComponent();

            pathToGit = new FileInfo(@"C:\Program Files (x86)\Git\bin\git.exe");
            try
            {
                DirectoryCatalog catalog =
                    new DirectoryCatalog(new FileInfo(Assembly.GetAssembly(typeof (MainWindow)).Location).DirectoryName);
                CompositionContainer container = new CompositionContainer(catalog);
                container.ComposeParts(this);
            }
            catch (CompositionException)
            {
                sqlParser = null;
            }

            var vm = new MainWindowViewModel(Properties.Settings.Default.Databases);
            vm.SourceChangeset = (DvcsScriptRepositoryBase.RevisionIdentifierBase)Properties.Settings.Default.LastChangeset ?? Properties.Settings.Default.LastTag;
            if (vm.SourceChangeset != null)
            {
                vm.SelectedSourceType = RevisionIdentifierConverter.GetLabelFromType(vm.SourceChangeset.GetType());
            }

            var riConverter = (RevisionIdentifierConverter)Resources["riConverter"];
            riConverter.DataContext = vm;
            DataContext = vm;
        }

        private void btAddDb_Click(object sender, RoutedEventArgs e)
        {
            var w = new DatabaseWindow();
            w.DataContext = DataContext;
            w.ShowDialog();
        }

        private void cbSourceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm != null && vm.SourceChangeset != null
                && vm.SelectedSourceType != RevisionIdentifierConverter.GetLabelFromType(vm.SourceChangeset.GetType()))
            {
                vm.SourceChangeset = null;
            }
        }

        private void btUpdate_Click(object sender, RoutedEventArgs e)
        {
            var dbSetting = ((MainWindowViewModel)DataContext).SelectedDatabase;
            var sourceChangeset = ((MainWindowViewModel)DataContext).SourceChangeset;
            ((MainWindowViewModel)DataContext).IsReady = false;
            RunTask(() => Update(dbSetting, sourceChangeset));
        }

        private void btBuild_Click(object sender, RoutedEventArgs e)
        {
            var items = ((MainWindowViewModel)DataContext).Items.Select(x => x.Item);
            var dbSetting = ((MainWindowViewModel)DataContext).SelectedDatabase;
            var sourceChangeset = ((MainWindowViewModel)DataContext).SourceChangeset;
            ((MainWindowViewModel) DataContext).IsReady = false;
            RunTask(() => Build(items, dbSetting, sourceChangeset));
        }

        private async Task Update(DatabaseSetting dbSetting, DvcsScriptRepositoryBase.RevisionIdentifierBase sourceChangeset)
        {
            ICollection<BuildItem> buildItems;
            using (var connection = new SqlConnection(dbSetting.ConnectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    var fileSystem = new FileSystem();
                    var db = dbSetting.Create(connection, transaction);
                    var gitProcess = new ExternalProcess(pathToGit.FullName);
                    var dvcsScriptRepo = new GitScriptRepository(dbSetting.ScriptsPath, dbSetting.ServerName, dbSetting.DatabaseName, gitProcess, fileSystem, sqlParser, new ConsoleLogger(SeverityLevel.Warning), false);
                    dvcsScriptRepo.SourceChangeset = sourceChangeset;

                    buildItems = await db.GetChangedBuildItemsAsync(dvcsScriptRepo);
                    transaction.Commit();
                }
            }

            await DispatcherInvoke(() =>
            {
                ((MainWindowViewModel)DataContext).IsReady = true;
                ((MainWindowViewModel)DataContext).IsDone = false;
                ObservableCollection<BuildItemViewModel> itemsCollection = new ObservableCollection<BuildItemViewModel>
                    (buildItems.Select(x => new BuildItemViewModel(x, Dispatcher)));
                ((MainWindowViewModel)DataContext).Items = itemsCollection;
            });
        }

        private async Task Build(IEnumerable<BuildItem> items, DatabaseSetting dbSetting, DvcsScriptRepositoryBase.RevisionIdentifierBase sourceChangeset)
        {
            using (var connection = new SqlConnection(dbSetting.ConnectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    var db = dbSetting.Create(connection, transaction);
                    
                    if (await db.BuildAsync(items))
                    {
                        transaction.Commit();
                    }
                }
            }

            await DispatcherInvoke(() =>
                {
                    ((MainWindowViewModel)DataContext).IsReady = true;
                    ((MainWindowViewModel)DataContext).IsDone = true;
                    var vm = ((MainWindowViewModel)DataContext);
                    Properties.Settings.Default.LastChangeset = vm.SourceChangeset as DvcsScriptRepositoryBase.ChangesetId;
                    Properties.Settings.Default.LastTag = vm.SourceChangeset as DvcsScriptRepositoryBase.Tag;
                    Properties.Settings.Default.Save();
                });
        }

        private async Task ShowExceptionMessage(Exception ex)
        {
            if (ex != null)
            {
                await DispatcherInvoke(() =>
                {
                    string message = "Unhandled exception occurred." + Environment.NewLine + ex.ToString();
                    MessageBox.Show(message, "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void RunTask(Action a)
        {
            Func<Task> wrappedAction = async () =>
            {
                Exception exception = null;
                try
                {
                    a();
                }
                catch (Exception e)
                {
                    exception = e;
                }
                await ShowExceptionMessage(exception);
            };
            RunTask(wrappedAction);
        }

        private void RunTask(Func<Task> t)
        {
            Func<Task> wrappedTask = async () =>
            {
                Exception exception = null;
                try
                {
                    await t();
                }
                catch (Exception e)
                {
                    exception = e;
                }
                await ShowExceptionMessage(exception);
            };

            Task.Run(wrappedTask);
        }

        private async Task DispatcherInvoke(Action a)
        {
            await Dispatcher.BeginInvoke(a);
        }
    }
}
