using Accord.MachineLearning.DecisionTrees;

namespace JadeChem.CustomControls.DiagramControls
{
    public partial class DecisionTreeViewControl : UserControl
    {
        private Font? boldFont;
        #region Constructor
        public DecisionTreeViewControl()
        {
            InitializeComponent();
            Disposed += (_, _) => boldFont?.Dispose();
        }
        #endregion

        #region Methods
        public void UpdateDecisionTreeView(DecisionTree decisionTree, string[] inputColumnNames, string[] classLabels)
        {
            if (decisionTree == null || inputColumnNames == null || classLabels == null)
                return;

            decisionTreeView.Nodes.Clear();
            boldFont?.Dispose();
            boldFont = new Font(Font, FontStyle.Bold);

            if (decisionTree.Root.IsLeaf)
            {
                TreeNode treeNode = decisionTreeView.Nodes.Add("Root");
                treeNode.ForeColor = Color.Brown;
                int classIndex = decisionTree.Root.Output ?? -1;
                treeNode.Nodes.Add(new TreeNode(GetClassLabel(classLabels, classIndex)));
                treeNode.Nodes[0].NodeFont = boldFont;

                decisionTreeView.ExpandAll();
                return;
            }

            TreeNode rootNode = decisionTreeView.Nodes.Add("Root");
            rootNode.ForeColor = Color.Brown;
            rootNode.NodeFont = boldFont;
            foreach (DecisionNode decisionNode in decisionTree.Root.Branches)
                rootNode.Nodes.Add(ConvertToTreeNode(decisionNode, inputColumnNames, classLabels));

            decisionTreeView.ExpandAll();
        }

        private TreeNode ConvertToTreeNode(DecisionNode decisionNode, string[] inputColumnNames, string[] classLabels)
        {
            string inputColumnName = inputColumnNames[decisionNode.Parent.Branches.AttributeIndex];
            string comparison = "";
            switch (decisionNode.Comparison)
            {
                case ComparisonKind.Equal:
                    comparison = "=";
                    break;
                case ComparisonKind.NotEqual:
                    comparison = "<>";
                    break;
                case ComparisonKind.GreaterThan:
                    comparison = ">";
                    break;
                case ComparisonKind.GreaterThanOrEqual:
                    comparison = ">=";
                    break;
                case ComparisonKind.LessThan:
                    comparison = "<";
                    break;
                case ComparisonKind.LessThanOrEqual:
                    comparison = "<=";
                    break;
            }
            double decisionNodeValue = decisionNode.Value ?? 0;
            string decisionNodeString = inputColumnName + " " + comparison + " " + decisionNodeValue.ToString("N4");
            TreeNode treeNode = new(decisionNodeString);

            if (!decisionNode.IsLeaf)
            {
                foreach (DecisionNode childNode in decisionNode.Branches)
                    treeNode.Nodes.Add(ConvertToTreeNode(childNode, inputColumnNames, classLabels));

                return treeNode;
            }
            else
            {
                int classIndex = decisionNode.Output ?? -1;
                treeNode.Nodes.Add(new TreeNode(GetClassLabel(classLabels, classIndex)));
                treeNode.Nodes[0].ForeColor = Color.Green;
                treeNode.Nodes[0].NodeFont = boldFont;
                return treeNode;
            }
        }

        private static string GetClassLabel(string[] classLabels, int classIndex) =>
            classIndex >= 0 && classIndex < classLabels.Length ? classLabels[classIndex] : "Unclassified";
        #endregion
    }
}
