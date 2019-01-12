using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Tetris {
    public class PieceToColorCellConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var t = (Piece.Types)value;
            switch (t) {
                case Piece.Types.I:
                    return new SolidColorBrush(Colors.Red);
                case Piece.Types.O:
                    return new SolidColorBrush(Colors.Blue);
                case Piece.Types.J:
                    return new SolidColorBrush(Colors.Yellow);
                case Piece.Types.L:
                    return new SolidColorBrush(Colors.Green);
                case Piece.Types.Z:
                    return new SolidColorBrush(Colors.Salmon);
                case Piece.Types.S:
                    return new SolidColorBrush(Colors.Orange);
                case Piece.Types.T:
                    return new SolidColorBrush(Colors.Cyan);
                case Piece.Types.Empty:
                    return new SolidColorBrush(Colors.Black);
                default:
                    throw new Exception($"unexpected piece type: {t}");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class PieceToColorBorderConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var t = (Piece.Types)value;
            return t == Piece.Types.Empty ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Gray);            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class DoubleToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var v = (double)value;
            return v.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
