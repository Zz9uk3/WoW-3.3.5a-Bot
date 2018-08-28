using AmeisenDB;
using AmeisenMapping.objects;
using System;
using System.Windows;

namespace DBTestGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string sqlConnectionString =
                "server=localhost;" +
                "database=ameisenbot;" +
                "uid=ameisenbot;" +
                "password=AmeisenPassword;";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            labelConnected.Content = "Connected:" + AmeisenDBManager.Instance.Connect(sqlConnectionString);
        }

        private void ButtonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            AmeisenDBManager.Instance.Disconnect();
        }

        MapNode nodeA, nodeB;

        private void ButtonAddNode_Click(object sender, RoutedEventArgs e)
        {
            nodeA = new MapNode(1,1,1);
            nodeB = new MapNode(2,2,1);

            AmeisenDBManager.Instance.UpdateOrAddNode(nodeA);
            AmeisenDBManager.Instance.UpdateOrAddNode(nodeB);
        }

        private void ButtonAddPath_Click(object sender, RoutedEventArgs e)
        {
            AmeisenDBManager.Instance.UpdateOrAddPath(new MapPath(nodeA, nodeB, 90));
        }
    }
}
