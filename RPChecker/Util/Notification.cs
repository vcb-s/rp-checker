// ****************************************************************************
//
// Copyright (C) 2014-2017 TautCony (TautCony@vcb-s.com)
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// ****************************************************************************

using System;
using System.Drawing;
using System.Windows.Forms;

namespace RPChecker.Util
{
    public static class Notification
    {
        public static DialogResult ShowError(string argMessage, Exception exception)
        {
            var ret = MessageBox.Show(caption: "RPChecker Error",
                text: $"{argMessage}:{Environment.NewLine}{exception.Message}"
#if DEBUG
                + $"{Environment.NewLine}{exception.StackTrace}", buttons: MessageBoxButtons.OK
#else
                ,buttons: MessageBoxButtons.YesNo
#endif
                , icon: MessageBoxIcon.Hand);
            if (ret != DialogResult.No) return ret;
            if (ShowInfo("保存调用堆栈至剪切板") == DialogResult.Yes)
            {
                Clipboard.SetText(exception.StackTrace);
            }
            return ret;
        }

        public static DialogResult ShowInfo(string argMessage, MessageBoxButtons buttons = MessageBoxButtons.YesNo)
        {
            return MessageBox.Show(caption: "RPChecker Info",
                text: argMessage,
                buttons: buttons, icon: MessageBoxIcon.Information);
        }


        public static string InputBox(string caption, string prompt, string defaultText)
        {
            string localInputText = defaultText;
            return InputQuery(caption, prompt, ref localInputText) ? localInputText : "";
        }

        private static int MulDiv(int number, float numerator, int denominator)
        {
            return (int)(number * numerator / denominator);
        }

        private static Size ScaleSize(Size size, float width, float height)
        {
            size.Height = (int)(size.Height*height);
            size.Width = (int) (size.Width* width);
            return size;
        }

        private static bool InputQuery(string caption, string prompt, ref string value)
        {
            var form = new Form
            {
                AutoScaleMode = AutoScaleMode.Font,
                Font = SystemFonts.IconTitleFont
            };

            var dialogUnits = form.AutoScaleDimensions;

            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.Text = caption;

            form.ClientSize = new Size(
                        MulDiv(180, dialogUnits.Width, 4),
                        MulDiv(63, dialogUnits.Height, 8));

            form.StartPosition = FormStartPosition.CenterScreen;

            var lblPrompt = new Label
            {
                Parent = form,
                AutoSize = true,
                Left = MulDiv(8, dialogUnits.Width, 4),
                Top = MulDiv(8, dialogUnits.Height, 8),
                Text = prompt
            };

            var edInput = new TextBox
            {
                Parent = form,
                Left = lblPrompt.Left,
                Top = MulDiv(19, dialogUnits.Height, 8),
                Width = MulDiv(164, dialogUnits.Width, 4),
                Text = value
            };
            edInput.SelectAll();


            int buttonTop = MulDiv(41, dialogUnits.Height, 8);
            //Command buttons should be 50x14 dlus
            Size buttonSize = ScaleSize(new Size(50, 14), dialogUnits.Width / 4, dialogUnits.Height / 8);

            Button bbOk = new Button
            {
                Parent = form,
                Text = "OK",
                DialogResult = DialogResult.OK
            };
            form.AcceptButton = bbOk;
            bbOk.Location = new Point(MulDiv(38, dialogUnits.Width, 4), buttonTop);
            bbOk.Size = buttonSize;

            Button bbCancel = new Button
            {
                Parent = form,
                Text = "Cancel",
                DialogResult = DialogResult.Cancel
            };
            form.CancelButton = bbCancel;
            bbCancel.Location = new Point(MulDiv(92, dialogUnits.Width, 4), buttonTop);
            bbCancel.Size = buttonSize;

            if (form.ShowDialog() == DialogResult.OK)
            {
                value = edInput.Text;
                return true;
            }
            return false;
        }
    }
}