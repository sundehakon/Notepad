using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAUISql.Data;
using MAUISql.Models;
using System.Collections.ObjectModel;

namespace MAUISql.ViewModels
{
    public partial class ProductsViewModel : ObservableObject
    {
        private readonly DatabaseContext _context;

        public ProductsViewModel(DatabaseContext context)
        {
            _context = context;
        }

        [ObservableProperty]
        private ObservableCollection<Note> _notes = new();

        [ObservableProperty]
        private Note _operatingProduct = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _busyText;
                
        public async Task LoadProductsAsync()
        {
            await ExecuteAsync(async () =>
            {
                var notes = await _context.GetAllAsync<Note>();
                if (notes is not null && notes.Any())
                {
                    Notes ??= new ObservableCollection<Note>();

                    foreach (var note in notes)
                    {
                        Notes.Add(note);
                    }
                }
            }, "Fetching notes...");
        }

        [RelayCommand]
        private void SetOperatingProduct(Note? note) => OperatingProduct = note ?? new();

        [RelayCommand]
        private async Task SaveProductAsync()
        {
            if (OperatingProduct is null)
                return;

            var busyText = OperatingProduct.Id == 0 ? "Creating note..." : "Updating note...";
            await ExecuteAsync(async () =>
            {
                if (OperatingProduct.Id == 0)
                {
                    // Create product
                    await _context.AddItemAsync<Note>(OperatingProduct);
                    Notes.Add(OperatingProduct);
                }
                else
                {
                    // Update product
                    if(await _context.UpdateItemAsync<Note>(OperatingProduct))
                    {
                        var productCopy = OperatingProduct.Clone();

                        var index = Notes.IndexOf(OperatingProduct);
                        Notes.RemoveAt(index);

                        Notes.Insert(index, productCopy);
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Note updation error", "Ok");
                        return;
                    }
                }
                SetOperatingProductCommand.Execute(new());
            }, busyText);
        }

        [RelayCommand]
        private async Task DeleteProductAsync(int id)
        {
            await ExecuteAsync(async () =>
            {
                if (await _context.DeleteItemByKeyAsync<Note>(id))
                {
                    var product = Notes.FirstOrDefault(p => p.Id == id);
                    Notes.Remove(product);
                }
                else
                {
                    await Shell.Current.DisplayAlert("Delete Error", "Product was not deleted", "Ok");
                }
            }, "Deleting product...");
        }

        private async Task ExecuteAsync(Func<Task> operation, string? busyText = null)
        {
            IsBusy = true;
            BusyText = busyText ?? "Processing...";
            try
            {
                await operation?.Invoke();
            }
            catch(Exception ex)
            {
                /*
                 * {System.TypeInitializationException: The type initializer for 'SQLite.SQLiteConnection' threw an exception.
                 ---> System.IO.FileNotFoundException: Could not load file or assembly 'SQLitePCLRaw.provider.dynamic_cdecl, Version=2.0.4.976, Culture=neutral, PublicKeyToken=b68184102cba0b3b' or one of its dependencies.
                File name: 'SQLitePCLRaw.provider.dynamic_cdecl, Version=2.0.4.976, Culture=neutral, PublicKeyToken=b68184102cba0b3b'
                   at SQLitePCL.Batteries_V2.Init()
                   at SQLite.SQLiteConnection..cctor()
                   --- End of inner exception stack trace ---
                   at SQLite.SQLiteConnectionWithLock..ctor(SQLiteConnectionString connectionString)
                   at SQLite.SQLiteConnectionPool.Entry..ctor(SQLiteConnectionString connectionString)
                   at SQLite.SQLiteConnectionPool.GetConnectionAndTransactionLock(SQLiteConnectionString connectionString, Object& transactionLock)
                   at SQLite.SQLiteConnectionPool.GetConnection(SQLiteConnectionString connectionString)
                   at SQLite.SQLiteAsyncConnection.GetConnection()
                   at SQLite.SQLiteAsyncConnection.<>c__DisplayClass33_0`1[[SQLite.CreateTableResult, SQLite-net, Version=1.8.116.0, Culture=neutral, PublicKeyToken=null]].<WriteAsync>b__0()
                   at System.Threading.Tasks.Task`1[[SQLite.CreateTableResult, SQLite-net, Version=1.8.116.0, Culture=neutral, PublicKeyToken=null]].InnerInvoke()
                   at System.Threading.Tasks.Task.<>c.<.cctor>b__273_0(Object obj)
                   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
                --- End of stack trace from previous location ---
                   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
                   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
                --- End of stack trace from previous location ---
                   at MAUISql.Data.DatabaseContext.<CreateTableIfNotExists>d__6`1[[MAUISql.Models.Product, MAUISql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext() in D:\MAUI\MAUISql\MAUISql\Data\DatabaseContext.cs:line 18
                   at MAUISql.Data.DatabaseContext.<GetTableAsync>d__7`1[[MAUISql.Models.Product, MAUISql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext() in D:\MAUI\MAUISql\MAUISql\Data\DatabaseContext.cs:line 23
                   at MAUISql.Data.DatabaseContext.<GetAllAsync>d__8`1[[MAUISql.Models.Product, MAUISql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext() in D:\MAUI\MAUISql\MAUISql\Data\DatabaseContext.cs:line 29
                   at MAUISql.ViewModels.ProductsViewModel.<LoadProductsAsync>b__6_0() in D:\MAUI\MAUISql\MAUISql\ViewModels\ProductsViewModel.cs:line 34
                   at MAUISql.ViewModels.ProductsViewModel.ExecuteAsync(Func`1 operation, String busyText) in D:\MAUI\MAUISql\MAUISql\ViewModels\ProductsViewModel.cs:line 103}
                 */
            }
            finally
            {
                IsBusy = false;
                BusyText = "Processing...";
            }
        }
    }
}
