using System;
using System.Collections.Generic;
using System.Linq;
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

namespace AudioIDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CompileWav CompileWav_;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += Loaded_Event;

            KeyDown += PressedKey;
        }

        void Loaded_Event(object sender, RoutedEventArgs e)
        {
            MainScopeVisual.Init();

            CompileWav_ = new CompileWav("C:/Users/Kree/Desktop/Modern General/Work/AudioCompiler/Debug/AudioCompiler.exe");
        }

        private void Compile_Click(object sender, RoutedEventArgs e)
        {
            CompileWav_.CreateExe(MainScopeVisual.InstructionsValues());
        }

        private void Wav_Click(object sender, RoutedEventArgs e)
        {
            CompileWav_.CreateWAV(MainScopeVisual.InstructionsValues());
        }

        private void PressedKey(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Delete)
            {
                MainScopeVisual.RemoveLastOperand();
            }

            //DEBUG printing
            if(e.Key == Key.D)
            {
                MainScopeVisual.WriteDebug();
            }
        }
    }
}
