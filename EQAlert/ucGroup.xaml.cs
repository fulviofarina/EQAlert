using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace EQAlert
{
    public sealed partial class ucGroup : UserControl
    {
        public ucGroup()
        {
            this.InitializeComponent();

            this.itemGridView.IsItemClickEnabled = true;
            this.itemListView.IsItemClickEnabled = true;
        }

        private ItemClickEventHandler groupClicked;

        public ItemClickEventHandler ItemClicked
        {
            get { return groupClicked; }
            set
            {
                if (groupClicked != null)
                {
                    this.itemListView.ItemClick -= groupClicked;
                    this.itemGridView.ItemClick -= groupClicked;
                }
                groupClicked = value;
                if (groupClicked != null)
                {
                    this.itemListView.ItemClick += groupClicked;
                    this.itemGridView.ItemClick += groupClicked;
                }
            }
        }
    }
}