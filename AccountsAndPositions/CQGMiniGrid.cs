using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;

/// <summary>
/// This class was created to provide the possibility of showing data without using any additional grid controls.
/// </summary>
namespace AccountsAndPositions
{
	public class CQGMiniGrid
	{
		private int m_Cols;
		private int m_Rows;
		private Panel m_Panel;
		private Label[][] m_Labels;
		
		private const int TOP = 20;
		private const int LEFT = 0;
		private const int HEIGHT = 18;
		private const int WIDTH = 60;
		
		/// <summary>
		/// Create labels and add them to a given groupbox.
		/// </summary>
		/// <param name="cols">
		/// Number of labels (vertical).
		/// </param>
		/// <param name="rows">
		/// Number of labels (horizontal).
		/// </param>
		/// <param name="parent">
		/// GroupBox, in which labels will be added.
		/// </param>
		public CQGMiniGrid(int cols, int rows, GroupBox parent)
		{
			if (cols < 1)
			{
				throw new ArgumentException(cols + " is invalid argument.");
			}
			if (rows < 1)
			{
				throw new ArgumentException(rows + " is invalid argument.");
			}
			
			m_Cols = cols;
			m_Rows = rows;
			
			m_Panel = new Panel();
			m_Panel.Height = parent.Size.Height - 48;
			m_Panel.Location = new Point(7, 38);
			m_Panel.BorderStyle = BorderStyle.None;
			m_Panel.AutoScroll = true;
			parent.Controls.Add(m_Panel);
			
			m_Labels = new Label[m_Cols + 1][];
			for (int j = 0; j <= m_Cols - 1; j++)
			{	
				m_Labels[j] = new Label[m_Rows+1];		
				for (int i = 0; i <= m_Rows; i++)
				{
					m_Labels[j][i] = new Label();
					m_Labels[j][i].Size = new Size(WIDTH, HEIGHT);
					m_Labels[j][i].UseMnemonic = false;
					m_Labels[j][i].BorderStyle = BorderStyle.Fixed3D;
					if (i == 0)
					{
						parent.Controls.Add(m_Labels[j][i]);
					}
					else
					{
						m_Panel.Controls.Add(m_Labels[j][i]);
					}
				}
			}
			ReDraw();
		}
		
		/// <summary>
		/// Recalculates sizes of labels and relocates all lables.
		/// </summary>
		public void ReDraw()
		{
			ReLocate();
			/// if autoscrolls are being shown, leaves the horizontal one only.
			if (m_Labels[0][m_Rows].Location.Y + HEIGHT > m_Panel.Height)
			{
				for (int i = 0; i <= m_Rows; i++)
				{
					m_Labels[m_Cols - 1][i].Width -= 16;
				}
				m_Panel.Width = m_Labels[m_Cols - 1][m_Rows].Location.X + 
								m_Labels[m_Cols - 1][m_Rows].Size.Width + 16;
			}
			else
			{
				m_Panel.Width = m_Labels[m_Cols - 1][m_Rows].Location.X + m_Labels[m_Cols - 1][m_Rows].Size.Width;
			}
			
		}
		
		/// <summary>
		/// Calculates locations of labels and sets their coordinates.
		/// </summary>
		private void ReLocate()
		{
			//Header part
			m_Labels[0][0].Location = new Point(m_Panel.Location.X, TOP);
			for (int j = 1; j <= m_Cols - 1; j++)
			{
				m_Labels[j][0].Location = new Point(m_Labels[j - 1][0].Location.X + 
													m_Labels[j - 1][0].Size.Width, TOP);
			}
			//Fields in Panel
			for (int i = 1; i <= m_Rows; i++)
			{
				m_Labels[0][i].Location = new Point(LEFT, HEIGHT *(i - 1));
				for (int j = 1; j <= m_Cols - 1; j++)
				{
					m_Labels[j][i].Location = new Point(m_Labels[j - 1][i].Location.X + 
														m_Labels[j - 1][i].Size.Width, 
														m_Labels[j - 1][i].Location.Y);
				}
			}
		}
		
		/// <summary>
		/// Sets the specified column's HeaderValue.
		/// </summary>
		/// <param name="col">
		/// The specified column.
		/// </param>
		/// <param name="val">
		/// New HeaderValue of the specified column.
		/// </param>
		public void SetHeaderValue(int col, string val)
		{
			if (col < 0 || col >= m_Cols)
			{
				throw (new IndexOutOfRangeException(col + " is invalid column index"));
			}
			m_Labels[col][0].Text = val;
			m_Labels[col][0].TextAlign = ContentAlignment.MiddleCenter;
			m_Labels[col][0].Font = new Font("Arial", 9, FontStyle.Bold);
		}
		
		/// <summary>
		/// Sets the specified column's alignment.
		/// </summary>
		/// <param name="col">
		/// The specified column.
		/// </param>
		/// <param name="val">
		/// New alignment of the specified column.
		/// </param>
		public void SetColumnAlign(int col, ContentAlignment val)
		{
			if (col < 0 || col >= m_Cols)
			{
				throw (new IndexOutOfRangeException(col + " is invalid column index"));
			}
			for (int i = 1; i <= m_Rows; i++)
			{
				m_Labels[col][i].TextAlign = val;
			}
		}
		
		/// <summary>
		/// Set specified column's width.
		/// </summary>
		/// <param name="col">
		/// The specified column.
		/// </param>
		/// <param name="val">
		/// New width of the specified column.
		/// </param>
		public void SetColumnWidth(int col, int val)
		{
			if (col < 0 || col >= m_Cols)
			{
				throw (new IndexOutOfRangeException(col + " is invalid column index"));
			}
			for (int i = 0; i <= m_Rows; i++)
			{
				m_Labels[col][i].Width = val;
			}
		}
		
		/// <summary>
		/// Clear the content of labels in the specified column.
		/// </summary>
		/// <param name="col">
		/// Column, content of which must be cleared.
		/// </param>
		public void ClearColumn(int col)
		{
			if (col < 0 || col >= m_Cols)
			{
				throw (new IndexOutOfRangeException(col + " is invalid column index"));
			}
			for (int i = 1; i <= m_Rows; i++)
			{
				m_Labels[col][i].Text = "";
			}
		}
		
		/// <summary>
		/// Clears the content of all labels.
		/// </summary>
		public void Clear()
		{
			for (int i = 0; i < m_Cols; i++)
			{
				ClearColumn(i);
			}
		}
		
		/// <summary>
		/// Gets the number of rows in grid.
		/// </summary>
		/// <returns>
		/// The number of rows in grid.
		/// </returns>
		public int Rows
		{
			get
			{
				return m_Rows;
			}
		}
		
		/// <summary>
		/// Gets/sets the item for the specified row and column.
		/// </summary>
		/// <param name="row">
		/// The specified row.
		/// </param>
		/// <param name="col">
		/// The specified column.
		/// </param>
		public Label this[int row, int col]
		{
			get
			{
				if (col < 0 || col >= m_Cols)
				{
					throw (new IndexOutOfRangeException(col + " is invalid column index"));
				}
				if (row < 0 || row >= m_Rows)
				{
					throw (new IndexOutOfRangeException(row + " is invalid row index"));
				}
				return m_Labels[col][row + 1];
			}
			set
			{
				if (col < 0 || col >= m_Cols)
				{
					throw (new IndexOutOfRangeException(col + " is invalid column index"));
				}
				if (row < 0 || row >= m_Rows)
				{
					throw (new IndexOutOfRangeException(row + " is invalid row index"));
				}
				m_Labels[col][row + 1] = value;
			}		
		}
	}
}
	

