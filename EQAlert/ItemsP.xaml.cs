using System;
using System.Collections.Generic;
using EQAlert.Data;
using Windows.UI.Xaml.Controls;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace EQAlert
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ItemsP : EQAlert.Common.LayoutAwarePage
    {
        public ItemsP()
        {
            this.InitializeComponent();

            this.ucGr.ItemClicked += aGroupClicked;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: Assign a bindable collection of items to this.DefaultViewModel["Items"]
            //  var sampleDataGroups = SampleDataSource.GetGroups((String)navigationParameter);

            var sampleDataGroups = SampleDataSource.GetGroups((String)navigationParameter);

            this.DefaultViewModel["Items"] = sampleDataGroups;

            //    = sampleDataGroups;

            //    this.DefaultViewModel["Items"] = sampleDataGroups;
        }

        private void aGroupClicked(object sender, ItemClickEventArgs e)
        {
            var groupId = ((SampleDataGroup)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(SplitPage), groupId);
        }
    }
}