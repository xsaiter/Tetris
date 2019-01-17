using System;
using System.ComponentModel;

namespace Tetris {
    public class NotifyBase : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class Cell : NotifyBase {
        public Cell(Piece.Types type) {
            _type = type;
        }

        public static Cell MakeEmpty() => new Cell(Piece.Types.Empty);
        public bool Empty => Type == Piece.Types.Empty;

        Piece.Types _type;

        public Piece.Types Type {
            get { return _type; }
            set {
                _type = value;
                RaisePropertyChanged(PROPERTY_TYPE_NAME);
            }

        }

        public const string PROPERTY_TYPE_NAME = nameof(Type);
    }

    public class Piece {
        public Types Type { get; set; }
        public int Orientation { get; set; }
        public int[] V { get; } = MakeItems();

        public static int[] MakeItems() => new int[N];
        public const int N = 4;

        static readonly Random _random = new Random(Guid.NewGuid().GetHashCode());

        public static Types GeneratePieceType() => (Types)_random.Next(TypesCount - 1);

        static int TypesCount => Enum.GetNames(typeof(Types)).Length;

        public void MakeNewType() => Type = GeneratePieceType();

        public enum Types {
            I,
            O,
            J,
            L,
            Z,
            S,
            T,
            Empty
        }
    }
}