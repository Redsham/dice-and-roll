using System;
using UnityEngine;


namespace Utilities
{
	public struct FlatArray2D<T>
	{
		public int Width  { get; private set; }
		public int Height { get; private set; }
		public T[] Data   { get; private set; }


		public FlatArray2D(int width, int height)
		{
			Width  = width;
			Height = height;
			Data   = new T[width * height];
		}

		// === Indexing ===

		public ref T this[int        x, int y] => ref Data[ToIndex(x, y)];
		public ref T this[int        index] => ref Data[index];
		public ref T this[Vector2Int coordinates] => ref this[coordinates.x, coordinates.y];

		// === Safe Indexing ===

		public bool TryGet(int x, int y, out T value)
		{
			if (IsInBounds(x, y)) {
				value = this[x, y];
				return true;
			}

			value = default;
			return false;
		}
		public bool TryGet(Vector2Int coordinates, out T value) => TryGet(coordinates.x, coordinates.y, out value);

		// === Settings ===
		
		public void Fill(T value) => Array.Fill(Data, value);
		public void ForEach(Action<T> action)
		{
			foreach (T t in Data) {
				action(t);
			}
		}
		public void Resize(int width, int height)
		{
			Width = width;
			Height = height;
			Data = new T[width * height];
		}
		
		// === Utilities ===

		public int        ToIndex(int       x, int y) => x + (y * Width);
		public Vector2Int ToCoordinates(int index) => new(index % Width, index / Width);

		public bool IsInBounds(int        x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
		public bool IsInBounds(Vector2Int coordinates) => IsInBounds(coordinates.x, coordinates.y);
	}
}