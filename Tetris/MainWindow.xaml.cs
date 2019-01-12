using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Tetris {
    public partial class MainWindow : Window {
        readonly DispatcherTimer _timer = new DispatcherTimer();
        
        readonly List<Cell> _cells = new List<Cell>();
        readonly Piece _piece = new Piece();
        readonly int[] _items = Piece.MakeItems();
        readonly List<int> _range = Enumerable.Range(0, Piece.N).ToList();

        int _rows, _cols, _n;
        bool _isActive;
        int _score = INITIAL_SCORE;
        int _speed = INITIAL_SPEED;

        const int INITIAL_SCORE = 0;
        const int INITIAL_SPEED = 7;

        public MainWindow() {
            InitializeComponent();
            SetPositionOfWindow();
            CreateBoard();            
        }

        void SetPositionOfWindow() {
            var screenx = SystemParameters.FullPrimaryScreenWidth;
            var screeny = SystemParameters.FullPrimaryScreenHeight;
            Left = (screenx - Width) / 2;
            Top = (screeny - Height) / 2;
        }

        void CreateBoard() {
            _rows = _grid.RowDefinitions.Count();
            _cols = _grid.ColumnDefinitions.Count();
            _n = _rows * _cols;

            var panels = new StackPanel[_n];
            var borders = new Border[_n];

            for (var i = 0; i < _rows; i++) {
                for (var j = 0; j < _cols; j++) {
                    _cells.Add(Cell.MakeEmpty());

                    var k = i * _cols + j;

                    panels[k] = new StackPanel();

                    borders[k] = new Border {
                        BorderBrush = Brushes.Red,
                        BorderThickness = new Thickness(2, 2, 2, 2),
                        Child = panels[k]
                    };

                    var bindingPanel = new Binding {
                        Source = _cells[k],
                        Path = new PropertyPath(Cell.PROPERTY_TYPE_NAME),
                        Mode = BindingMode.OneWay,
                        Converter = new PieceToColorCellConverter()
                    };
                    panels[k].SetBinding(BackgroundProperty, bindingPanel);

                    var bindingBorder = new Binding {
                        Source = _cells[k],
                        Path = new PropertyPath(Cell.PROPERTY_TYPE_NAME),
                        Mode = BindingMode.OneWay,
                        Converter = new PieceToColorBorderConverter()
                    };
                    borders[k].SetBinding(BorderBrushProperty, bindingBorder);

                    Grid.SetRow(borders[k], i);
                    Grid.SetColumn(borders[k], j);

                    _grid.Children.Add(borders[k]);
                }
            }

            _timer.Interval = CreateIntervalTimer();
            _timer.Tick += OnTimerTick;

            UpdateScore(INITIAL_SCORE);
            UpdateSpeed(INITIAL_SPEED);
        }

        void UpdateSpeed(int speed) {
            _speed = speed;
            SpeedTextBox.Text = _speed.ToString();
        }

        void UpdateScore(int score) {
            _score = score;
            ScoreTextBox.Text = _score.ToString();
        }

        void NewGame(object sender, RoutedEventArgs e) {
            ResetBoard();
            StartGame();
            NewPiece();
            ShowPiece();
        }

        void StartGame() {
            _isActive = true;
            _timer.Start();
        }

        void NewPiece() {
            _piece.MakeNewType();
            GetMaster().MakePiece();
        }        

        void OnTimerTick(object sender, EventArgs e) {
            ResetPiece();

            PreMoveToDown();

            if (CanMoveToDown()) {
                MovePiece();
            } else {
                ShowPiece();

                RemoveCompletedLines();

                NewPiece();

                if (IsGameOver()) {
                    GameOver();
                    return;
                }
            }

            ShowPiece();
        }

        void PreMoveToDown() => _range.ForEach(i => _items[i] = _piece.Items[i] + _cols);

        bool CanMoveToDown() => _range.All(i => (_items[i] / _cols < _rows) && _cells[_items[i]].Empty);

        void RemoveCompletedLines() {
            var need = true;
            var score = _score;
            for (var i = 0; i < _cells.Count; ++i) {
                if (need && _cells[i].Empty) {
                    need = false;
                }
                if ((i + 1) % _cols == 0) {
                    if (need) {
                        for (var j = i; j >= _cols; --j) {
                            _cells[j].Type = _cells[j - _cols].Type;
                        }
                        ++score;
                        UpdateScore(score);
                    }
                    need = true;
                }
            }
        }

        void ShowPiece() {
            SetItemTypes(_piece.Type);
        }

        void MovePiece() {
            _range.ForEach(i => _piece.Items[i] = _items[i]);
        }

        void ResetPiece() {
            SetItemTypes(Piece.Types.Empty);
        }

        void SetItemTypes(Piece.Types type) {
            _range.ForEach(i => _cells[_piece.Items[i]].Type = type);
        }

        void ResetBoard() {
            _cells.ForEach(c => c.Type = Piece.Types.Empty);
        }

        bool IsGameOver() {
            return _range.Any(i => !_cells[_piece.Items[i]].Empty);
        }

        void GameOver() {
            _isActive = false;
            _timer.Stop();
            MessageBox.Show("game over!");
        }

        void Window_KeyDown(object sender, KeyEventArgs e) {
            if (_piece != null) {
                switch (e.Key) {
                    case Key.Up:
                        Rotate();
                        ShowPiece();
                        break;
                    case Key.Down:
                        Pause();
                        break;
                    case Key.Left:
                        MoveToLeft();
                        ShowPiece();
                        break;
                    case Key.Right:
                        MoveToRight();
                        ShowPiece();
                        break;
                }
            }
        }

        void MoveToRight() {
            ResetPiece();
            PreMoveToRight();
            if (CanMoveToRight()) {
                MovePiece();
            }
        }

        void PreMoveToRight() => _range.ForEach(i => _items[i] = _piece.Items[i] + 1);

        bool CanMoveToRight() => CheckBounds() && _range.All(i => Delta(i) > 0);

        void MoveToLeft() {
            ResetPiece();
            PreMoveToLeft();
            if (CanMoveToLeft()) {
                MovePiece();
            }
        }

        void PreMoveToLeft() => _range.ForEach(i => _items[i] = _piece.Items[i] - 1);

        bool CanMoveToLeft() => CheckBounds() && _range.All(i => Delta(i) < 0);

        int Delta(int i) => _items[i] % _cols - _piece.Items[i] % _cols;

        void Pause() {
            if (_timer.IsEnabled) {
                _timer.Stop();
            } else {
                _timer.Start();
            }
        }

        void Rotate() {
            ResetPiece();
            GetMaster().Rotate();
        }

        bool CheckBounds() => _range.All(i => _items[i] >= 0 && _items[i] < _n && _cells[_items[i]].Empty);

        void OnPause(object sender, RoutedEventArgs e) {
            if (_isActive) {
                if (_timer.IsEnabled) {
                    _timer.Stop();
                } else {
                    _timer.Start();
                }
            }
        }

        void OnOptions(object sender, RoutedEventArgs e) {
            if (_timer.IsEnabled) {
                _timer.Stop();
            }

            var window = new OptionsWindow(_speed);
            if (window.ShowDialog().Value) {
                var speed = Convert.ToInt32(window.SliderSpeed.Value);
                UpdateSpeed(speed);
                _timer.Interval = CreateIntervalTimer();
            }

            if (_isActive) {
                _timer.Start();
            }
        }

        TimeSpan CreateIntervalTimer() => new TimeSpan(0, 0, 0, 0, 20 * (10 - _speed + 1));

        void OnExit(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        public class Master {
            public Action MakePiece { get; set; }
            public Action Rotate { get; set; }
        }

        Master GetMaster() {
            var master = new Master();
            switch (_piece.Type) {
                case Piece.Types.I:
                    Prepare_I(master);
                    break;
                case Piece.Types.O:
                    Prepare_O(master);
                    break;
                case Piece.Types.J:
                    Prepare_J(master);
                    break;
                case Piece.Types.L:
                    Prepare_L(master);
                    break;
                case Piece.Types.Z:
                    Prepare_Z(master);
                    break;
                case Piece.Types.S:
                    Prepare_S(master);
                    break;
                case Piece.Types.T:
                    Prepare_T(master);
                    break;
                default:
                    throw new Exception($"unexpected piece type: {_piece.Type}");
            }
            return master;
        }

        void PreMakePiece() {
            _piece.Orientation = 0;
            _piece.Items[0] = (_cols - 4) / 2;
        }

        void Prepare_I(Master master) {
            master.MakePiece = () => {
                PreMakePiece();
                _piece.Items[1] = _piece.Items[0] + 1;
                _piece.Items[2] = _piece.Items[1] + 1;
                _piece.Items[3] = _piece.Items[2] + 1;
            };
            master.Rotate = () => {
                switch (_piece.Orientation) {
                    case 0:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] + _cols - 1;
                        _items[2] = _piece.Items[2] + 2 * _cols - 2;
                        _items[3] = _piece.Items[3] + 3 * _cols - 3;
                        break;
                    case 1:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] - _cols + 1;
                        _items[2] = _piece.Items[2] - 2 * _cols + 2;
                        _items[3] = _piece.Items[3] - 3 * _cols + 3;
                        break;
                }
                Rotate(2);
            };
        }

        void Prepare_O(Master master) {
            master.MakePiece = () => {
                PreMakePiece();
                _piece.Items[1] = _piece.Items[0] + 1;
                _piece.Items[2] = _piece.Items[0] + _cols;
                _piece.Items[3] = _piece.Items[1] + _cols;
            };
            master.Rotate = () => { };
        }

        void Prepare_J(Master master) {
            master.MakePiece = () => {
                PreMakePiece();
                _piece.Items[1] = _piece.Items[0] + 1;
                _piece.Items[2] = _piece.Items[0] + _cols;
                _piece.Items[3] = _piece.Items[2] + _cols;
            };
            master.Rotate = () => {
                switch (_piece.Orientation) {
                    case 0:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] + _cols - 1;
                        _items[2] = _piece.Items[2] - _cols - 1;
                        _items[3] = _piece.Items[3] - 2 * _cols - 2;
                        break;
                    case 1:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] - _cols - 1;
                        _items[2] = _piece.Items[2] - _cols + 1;
                        _items[3] = _piece.Items[3] - 2 * _cols + 2;
                        break;
                    case 2:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] - _cols + 1;
                        _items[2] = _piece.Items[2] + _cols + 1;
                        _items[3] = _piece.Items[3] + 2 * _cols + 2;
                        break;
                    case 3:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] + _cols + 1;
                        _items[2] = _piece.Items[2] + _cols - 1;
                        _items[3] = _piece.Items[3] + 2 * _cols - 2;
                        break;
                }
                Rotate(4);
            };
        }

        void Prepare_L(Master master) {
            master.MakePiece = () => {
                PreMakePiece();
                _piece.Items[1] = _piece.Items[0] - 1;
                _piece.Items[2] = _piece.Items[0] + _cols;
                _piece.Items[3] = _piece.Items[2] + _cols;
            };
            master.Rotate = () => {
                switch (_piece.Orientation) {
                    case 0:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] - _cols + 1;
                        _items[2] = _piece.Items[2] - _cols - 1;
                        _items[3] = _piece.Items[3] - 2 * _cols - 2;
                        break;
                    case 1:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] + _cols + 1;
                        _items[2] = _piece.Items[2] - _cols + 1;
                        _items[3] = _piece.Items[3] - 2 * _cols + 2;
                        break;
                    case 2:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] + _cols - 1;
                        _items[2] = _piece.Items[2] + _cols + 1;
                        _items[3] = _piece.Items[3] + 2 * _cols + 2;
                        break;
                    case 3:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] - _cols - 1;
                        _items[2] = _piece.Items[2] + _cols - 1;
                        _items[3] = _piece.Items[3] + 2 * _cols - 2;
                        break;
                }
                Rotate(4);
            };
        }

        void Prepare_Z(Master master) {
            master.MakePiece = () => {
                PreMakePiece();
                _piece.Items[1] = _piece.Items[0] - 1;
                _piece.Items[2] = _piece.Items[0] + _cols;
                _piece.Items[3] = _piece.Items[0] + _cols + 1;
            };
            master.Rotate = () => {
                switch (_piece.Orientation) {
                    case 0:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] - _cols + 1;
                        _items[2] = _piece.Items[2] - _cols - 1;
                        _items[3] = _piece.Items[3] - 2;
                        break;
                    case 1:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] + _cols - 1;
                        _items[2] = _piece.Items[2] + _cols + 1;
                        _items[3] = _piece.Items[3] + 2;
                        break;
                }
                Rotate(2);
            };
        }

        void Prepare_S(Master master) {
            master.MakePiece = () => {
                PreMakePiece();
                _piece.Items[1] = _piece.Items[0] + 1;
                _piece.Items[2] = _piece.Items[0] + _cols;
                _piece.Items[3] = _piece.Items[0] + _cols - 1;
            };
            master.Rotate = () => {
                switch (_piece.Orientation) {
                    case 0:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] + _cols - 1;
                        _items[2] = _piece.Items[2] - _cols - 1;
                        _items[3] = _piece.Items[3] - 2 * _cols;
                        break;
                    case 1:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] - _cols + 1;
                        _items[2] = _piece.Items[2] + _cols + 1;
                        _items[3] = _piece.Items[3] + 2 * _cols;
                        break;
                }
                Rotate(2);
            };
        }

        void Prepare_T(Master master) {
            master.MakePiece = () => {
                PreMakePiece();
                _piece.Items[1] = _piece.Items[0] - 1;
                _piece.Items[2] = _piece.Items[0] + 1;
                _piece.Items[3] = _piece.Items[0] + _cols;
            };
            master.Rotate = () => {
                switch (_piece.Orientation) {
                    case 0:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] - _cols + 1;
                        _items[2] = _piece.Items[2] + _cols - 1;
                        _items[3] = _piece.Items[3] - _cols - 1;
                        break;
                    case 1:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] + _cols + 1;
                        _items[2] = _piece.Items[2] - _cols - 1;
                        _items[3] = _piece.Items[3] - _cols + 1;
                        break;
                    case 2:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] + _cols - 1;
                        _items[2] = _piece.Items[2] - _cols + 1;
                        _items[3] = _piece.Items[3] + _cols + 1;
                        break;
                    case 3:
                        _items[0] = _piece.Items[0];
                        _items[1] = _piece.Items[1] - _cols - 1;
                        _items[2] = _piece.Items[2] + _cols + 1;
                        _items[3] = _piece.Items[3] + _cols - 1;
                        break;
                }
                Rotate(4);
            };
        }

        void Rotate(int countOrientations) {
            if (CanRotate()) {
                MovePiece();
                ChangeOrientation(countOrientations);
            }
        }

        bool CanRotate() => CheckBounds() && _range.All(i => Math.Abs(Delta(i)) < Piece.N);

        void ChangeOrientation(int countOrientations) {
            _piece.Orientation++;
            if (_piece.Orientation == countOrientations) {
                _piece.Orientation = 0;
            }
        }
    }
}