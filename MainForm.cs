using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseKeyStatus
{
   // Created by Taber Henderson
   // 20 Febuary 2026

   // TODO:
   // * Right-click context menu for options (and "help > about" menu)
   // * Enable different color themes
   // * Allow other key states to be tracked and displayed (arrow, function, others shown by name...)

   public partial class MainForm : Form
   {
      Timer updateTimer;

      Keys modifierState;
      MouseButtons buttonState;

      Point mouseStart;

      public MainForm()
      {
         InitializeComponent();

         this.Text = "Mouse Key Status - v0.2";
         this.Size = new Size( 240, 160 );
         this.MinimumSize = new Size( 150, 100 );

         this.FormBorderStyle = FormBorderStyle.Sizable;
         this.SizeGripStyle = SizeGripStyle.Show;
         this.MaximizeBox = false;
         this.MinimizeBox = false;

         this.DoubleBuffered = true;
         this.ResizeRedraw = true;
      }

      protected override void OnLoad( EventArgs e )
      {
         base.OnLoad( e );

         this.updateTimer = new Timer();
         this.updateTimer.Interval = 100;
         this.updateTimer.Tick += UpdateTimer_Tick;
         this.updateTimer.Start();
      }

      void UpdateTimer_Tick( object sender, EventArgs e )
      {
         // get new modifier key and mouse button states
         Keys newModifierState = Control.ModifierKeys;
         MouseButtons newButtonState = Control.MouseButtons;

         // track if state changed
         bool stateChanged = newModifierState != this.modifierState || newButtonState != this.buttonState;

         // update current states
         this.modifierState = newModifierState;
         this.buttonState = newButtonState;

         // only invalidate if state changed
         if( stateChanged )
         {
            this.Invalidate();
         }
      }

      protected override void OnMouseDown( MouseEventArgs e )
      {
         base.OnMouseDown( e );

         if( e.Button == MouseButtons.Left )
         {
            this.mouseStart = e.Location;
         }
      }

      protected override void OnMouseMove( MouseEventArgs e )
      {
         base.OnMouseMove( e );

         if( e.Button == MouseButtons.Left )
         {
            this.Cursor = Cursors.SizeAll;

            int xOffset = e.X - this.mouseStart.X;
            int yOffset = e.Y - this.mouseStart.Y;
            this.Location = new Point( this.Left + xOffset, this.Top + yOffset );
         }
         else
         {
            this.Cursor = Cursors.Default;
         }
      }

      protected override void OnMouseDoubleClick( MouseEventArgs e )
      {
         base.OnMouseDoubleClick( e );

         Size borderSize = SystemInformation.BorderSize;
         Size frameSize = SystemInformation.FrameBorderSize;
         int captionHeight = SystemInformation.CaptionHeight;
         int toolWinCaptionHeight = SystemInformation.ToolWindowCaptionHeight;

         if( e.Button == MouseButtons.Left )
         {
            if( this.FormBorderStyle == FormBorderStyle.None )
            {
               this.FormBorderStyle = FormBorderStyle.Sizable;
               this.TransparencyKey = Color.Empty;
               this.TopMost = false;
               this.ShowInTaskbar = true;

               // adjust window location to retain current relative position
               this.Location = new Point( this.Left - frameSize.Width, this.Top - captionHeight - frameSize.Height );
            }
            else
            {
               this.FormBorderStyle = FormBorderStyle.None;
               this.TransparencyKey = SystemColors.Control;
               this.TopMost = true;
               this.ShowInTaskbar = false;

               // adjust window location to retain current relative position
               this.Location = new Point( this.Left + frameSize.Width, this.Top + captionHeight + frameSize.Height );
            }
         }
      }

      Brush GetFillBrush( MouseButtons button )
      {
         return this.buttonState.HasFlag( button ) ? Brushes.SteelBlue : Brushes.LightGray;
      }

      Brush GetFillBrush( Keys modKey )
      {
         return this.modifierState.HasFlag( modKey ) ? Brushes.SteelBlue : Brushes.LightGray;
      }

      protected override void OnPaint( PaintEventArgs e )
      {
         base.OnPaint( e );

         Graphics grfx = e.Graphics;

         grfx.SmoothingMode = SmoothingMode.AntiAlias;
         grfx.Clear( this.BackColor );

         int mouseButtons = SystemInformation.MouseButtons;
         bool buttonsSwapped = SystemInformation.MouseButtonsSwapped;

         int width = this.ClientSize.Width;
         int height = this.ClientSize.Height;

         int mouseAreaWidth = (int)( width * ( 1f / 2f ) );
         int keyAreaWidth = (int)( width * ( 1f / 2f ) );

         int border = (int)( mouseAreaWidth * 0.05f );

         int buttonGapCount = 2;
         int buttonWidth = ( mouseAreaWidth - border * buttonGapCount - border * 2 ) / 3;
         int buttonHeight = (int)( height * 0.75f ); //height - border * 2;
         int cornerRad = (int)( buttonWidth * 0.25f );

         int baseKeyWidth = (int)( keyAreaWidth - border * 2 );
         int KeyWidthMinor = (int)( ( baseKeyWidth - border ) / 2f );
         int keyWidth = (int)( keyAreaWidth * 0.80f );
         int keyHeight = (int)( height * 0.30f );

         int currX = border * 1;
         int currY = (int)( height - buttonHeight ) / 2; // border * 1;

         Brush fillBrush;
         Pen borderPen = new Pen( Color.Black, 1f );
         Font keyFont = new Font( this.Font.FontFamily, keyHeight * 0.35f, FontStyle.Bold, GraphicsUnit.Pixel );

         // draw mouse buttons
         // - left
         fillBrush = GetFillBrush( MouseButtons.Left );
         GraphicsPath roundRectPath = CreateRoundedRectPath( currX, currY, buttonWidth, buttonHeight, cornerRad, CornerFlags.TopLeft | CornerFlags.BottomLeft );
         grfx.FillPath( fillBrush, roundRectPath );
         grfx.DrawPath( borderPen, roundRectPath );
         roundRectPath.Dispose();
         grfx.DrawString( "L", keyFont, Brushes.Black, currX + border, currY + buttonHeight / 2 - border );
         currX += buttonWidth + border;
         // - middle
         fillBrush = GetFillBrush( MouseButtons.Middle );
         roundRectPath = CreateRoundedRectPath( currX, currY, buttonWidth, buttonHeight, cornerRad, CornerFlags.All );
         grfx.FillPath( fillBrush, roundRectPath );
         grfx.DrawPath( borderPen, roundRectPath );
         roundRectPath.Dispose();
         grfx.DrawString( "M", keyFont, Brushes.Black, currX + border, currY + buttonHeight / 2 - border );
         currX += buttonWidth + border;
         // - right
         fillBrush = GetFillBrush( MouseButtons.Right );
         roundRectPath = CreateRoundedRectPath( currX, currY, buttonWidth, buttonHeight, cornerRad, CornerFlags.TopRight | CornerFlags.BottomRight );
         grfx.FillPath( fillBrush, roundRectPath );
         grfx.DrawPath( borderPen, roundRectPath );
         roundRectPath.Dispose();
         grfx.DrawString( "R", keyFont, Brushes.Black, currX + border, currY + buttonHeight / 2 - border );
         currX += buttonWidth + border;

         currX += border;
         currY = (int)( height / 2f - border / 2f - keyHeight );

         // draw modifier keys
         // - shift
         fillBrush = GetFillBrush( Keys.Shift );
         grfx.FillRectangle( fillBrush, currX, currY, keyWidth, keyHeight );
         grfx.DrawRectangle( borderPen, currX, currY, keyWidth, keyHeight );
         grfx.DrawString( "Shift", keyFont, Brushes.Black, currX + border, currY + border );
         currY += keyHeight + border;
         // - ctrl
         fillBrush = GetFillBrush( Keys.Control );
         grfx.FillRectangle( fillBrush, currX, currY, KeyWidthMinor, keyHeight );
         grfx.DrawRectangle( borderPen, currX, currY, KeyWidthMinor, keyHeight );
         grfx.DrawString( "Ctrl", keyFont, Brushes.Black, currX + border, currY + border );
         currX += KeyWidthMinor + border;
         // - alt
         fillBrush = GetFillBrush( Keys.Alt );
         grfx.FillRectangle( fillBrush, currX, currY, KeyWidthMinor, keyHeight );
         grfx.DrawRectangle( borderPen, currX, currY, KeyWidthMinor, keyHeight );
         grfx.DrawString( "Alt", keyFont, Brushes.Black, currX + border, currY + border );

         borderPen.Dispose();
         keyFont.Dispose();
      }

      [Flags]
      enum CornerFlags
      {
         None = 0,
         TopLeft = 1,
         TopRight = 2,
         BottomRight = 4,
         BottomLeft = 8,
         All = TopLeft | TopRight | BottomRight | BottomLeft,
      }

      GraphicsPath CreateRoundedRectPath( int x, int y, int width, int height,
         int cornerRadius, CornerFlags corners = CornerFlags.All )
      {
         GraphicsPath path = new GraphicsPath();

         int diam = cornerRadius * 2;

         int topEdgeLeft = x;
         int topEdgeRight = x + width;

         int bottomEdgeLeft = x;
         int bottomEdgeRight = x + width;

         int leftEdgeTop = y;
         int leftEdgeBottom = y + height;

         int rightEdgeTop = y;
         int rightEdgeBottom = y + height;

         if( corners.HasFlag( CornerFlags.TopLeft ) )
         {
            leftEdgeTop += cornerRadius;
            topEdgeLeft += cornerRadius;
         }
         if( corners.HasFlag( CornerFlags.TopRight ) )
         {
            topEdgeRight -= cornerRadius;
            rightEdgeTop += cornerRadius;
         }
         if( corners.HasFlag( CornerFlags.BottomRight ) )
         {
            bottomEdgeRight -= cornerRadius;
            rightEdgeBottom -= cornerRadius;
         }
         if( corners.HasFlag( CornerFlags.BottomLeft ) )
         {
            bottomEdgeLeft += cornerRadius;
            leftEdgeBottom -= cornerRadius;
         }

         if( corners.HasFlag( CornerFlags.TopLeft ) )
         {
            path.AddArc( x, y, diam, diam, 180f, 90f );
         }
         path.AddLine( topEdgeLeft, y, topEdgeRight, y );
         if( corners.HasFlag( CornerFlags.TopRight ) )
         {
            path.AddArc( x + width - diam, y, diam, diam, 270f, 90f );
         }
         path.AddLine( x + width, rightEdgeTop, x + width, rightEdgeBottom );
         if( corners.HasFlag( CornerFlags.BottomRight ) )
         {
            path.AddArc( x + width - diam, y + height - diam, diam, diam, 0f, 90f );
         }
         path.AddLine( bottomEdgeRight, y + height, bottomEdgeLeft, y + height );
         if( corners.HasFlag( CornerFlags.BottomLeft ) )
         {
            path.AddArc( x, y + height - diam, diam, diam, 90f, 90f );
         }
         path.AddLine( x, leftEdgeBottom, x, leftEdgeTop );

         path.CloseFigure();

         return path;
      }

   }
}