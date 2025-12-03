using ARM.Models;
using ARM.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarfBuzzSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ARM.ViewModels.Directories
{
    partial class DirectoryPostsViewModel : ObservableObject
    {
        private readonly PostgresDBService _dbService;

        public List<LookupItem> FactVMethods { get; } = Lookups.FactVMethods;
        public List<LookupItem> FactWMethods { get; } = Lookups.FactWMethods;
        public List<LookupItem> Directions { get; } = Lookups.Directions;
        public List<LookupItem> MachineTypes { get; } = Lookups.MachineTypes;
        public List<LookupItem> CtrlTypes { get; } = Lookups.CtrlTypes;
        public List<LookupItem> UpDownFills { get; } = Lookups.UpDownFills;
        public List<LookupItem> UserTypedTemperatures { get; } = Lookups.UserTypedTemperatures;
        public List<LookupItem> StartReverseds { get; } = Lookups.StartReverseds;

        public DirectoryPostsViewModel(PostgresDBService dbService)
        {
            _dbService = dbService;
            Posts = new ObservableCollection<PostModel>();

            AddCommand = new RelayCommand(OnAdd);
            SaveCommand = new RelayCommand(async () => await OnSaveAsync());
            DeleteCommand = new RelayCommand(async () => await OnDeleteAsync(), () => SelectedPost != null);
            CancelCommand = new RelayCommand(OnCancel);

            _ = LoadPostsAsync();
        }

        public ObservableCollection<PostModel> Posts { get; }

        [ObservableProperty]
        private PostModel selectedPost;

        private PostModel _addedItem;

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }


        private async Task LoadPostsAsync()
        {
            var posts = await _dbService.GetPostsAsync();
            Posts.Clear();
            foreach (var p in posts)
            {
                // присваиваем списки для ComboBox
                p.FactVMethodsLookup = FactVMethods;
                p.FactWMethodsLookup = FactWMethods;
                p.DirectionsLookup = Directions;
                p.MachineTypesLookup = MachineTypes;
                p.CtrlTypesLookup = CtrlTypes;
                p.UpDownFillsLookup = UpDownFills;
                p.UserTypedTemperaturesLookup = UserTypedTemperatures;
                p.StartReversedsLookup = StartReverseds;

                // проставляем отображаемый текст по Id (LookUp)
                p.LookupFactVMethod = FactVMethods.FirstOrDefault(x => x.Id == p.FactVMethod)?.Name ?? "";
                p.LookupFactWMethod = FactWMethods.FirstOrDefault(x => x.Id == p.FactWMethod)?.Name ?? "";
                p.LookupDirection = Directions.FirstOrDefault(x => x.Id == p.Direction)?.Name ?? "";
                p.LookupMachineType = MachineTypes.FirstOrDefault(x => x.Id == p.MachineType)?.Name ?? "";
                p.LookupCtrlType = CtrlTypes.FirstOrDefault(x => x.Id == p.CtrlType)?.Name ?? "";
                p.LookupUpDownFill = UpDownFills.FirstOrDefault(x => x.Id == p.UpDownFill)?.Name ?? "";
                p.LookupUserTypedTemperature = UserTypedTemperatures.FirstOrDefault(x => x.Id == (p.UserTypedTemperature == true ? 1 : 0))?.Name ?? "";
                p.LookupStartReversed = StartReverseds.FirstOrDefault(x => x.Id == p.StartReversed)?.Name ?? "";

                Posts.Add(p);
            }
        }

        private void OnAdd()
        {
            if (_addedItem != null)
                return;

            _addedItem = new PostModel()
            {
                FactVMethodsLookup = FactVMethods,
                FactWMethodsLookup = FactWMethods,
                DirectionsLookup = Directions,
                MachineTypesLookup = MachineTypes,
                CtrlTypesLookup = CtrlTypes,
                UpDownFillsLookup = UpDownFills,
                UserTypedTemperaturesLookup = UserTypedTemperatures,
                StartReversedsLookup = StartReverseds
            };

            Posts.Add(_addedItem);
            SelectedPost = _addedItem;
        }

        private async Task OnSaveAsync()
        {
            var item = SelectedPost;
            if (item == null) return;

            var isNew = ReferenceEquals(item, _addedItem) || item.Post == 0;

            if (isNew)
            {
                var newId = await _dbService.InsertPostAsync(item);
                if (newId > 0)
                    item.Post = (short)newId;

                _addedItem = null;
            }
            else
            {
                await _dbService.UpdatePostAsync(item);
            }
            await LoadPostsAsync();
        }

        private async Task OnDeleteAsync()
        {
            if (SelectedPost == null) return;

            await _dbService.DeletePostAsync(SelectedPost.Post);
            Posts.Remove(SelectedPost);
            SelectedPost = null;
        }

        private void OnCancel()
        {
            if (_addedItem != null)
            {
                Posts.Remove(_addedItem);
                _addedItem = null;
            }
        }
    }
}
