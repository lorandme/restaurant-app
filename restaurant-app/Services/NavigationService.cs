using System;
using System.Collections.Generic;
using System.Windows.Controls;
using restaurant_app.Helpers;
//using restaurant_app.ViewModels;

namespace restaurant_app.Services
{
    public class NavigationService
    {
        private readonly Dictionary<string, Func<ViewModelBase>> _viewModels = new Dictionary<string, Func<ViewModelBase>>();
        private readonly Frame _navigationFrame;
        private readonly Dictionary<Type, Type> _viewModelToViewMap = new Dictionary<Type, Type>();

        public ViewModelBase CurrentViewModel { get; private set; }

        public NavigationService(Frame navigationFrame)
        {
            _navigationFrame = navigationFrame ?? throw new ArgumentNullException(nameof(navigationFrame));
        }

        public void RegisterViewModel<TViewModel>(string key, Func<TViewModel> createViewModel)
            where TViewModel : ViewModelBase
        {
            _viewModels[key] = createViewModel;
        }

        public void RegisterViewForViewModel<TViewModel, TView>()
            where TViewModel : ViewModelBase
            where TView : Page
        {
            _viewModelToViewMap[typeof(TViewModel)] = typeof(TView);
        }

        public bool NavigateTo(string viewModelKey)
        {
            if (!_viewModels.ContainsKey(viewModelKey))
                return false;

            var viewModel = _viewModels[viewModelKey]();
            CurrentViewModel = viewModel;

            if (_viewModelToViewMap.TryGetValue(viewModel.GetType(), out var viewType))
            {
                var view = Activator.CreateInstance(viewType) as Page;
                if (view != null)
                {
                    view.DataContext = viewModel;
                    _navigationFrame.Navigate(view);
                    return true;
                }
            }

            return false;
        }
    }
}