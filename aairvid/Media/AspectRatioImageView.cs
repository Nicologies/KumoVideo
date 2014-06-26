using Android.Content;
using Android.Util;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid.UIUtils
{
    public class AspectRatioImageView : ImageView
    {
        public AspectRatioImageView(Context context)
            : base(context)
        {

        }

        public AspectRatioImageView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {

        }

        public AspectRatioImageView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {

        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (Drawable != null)
            {
                int width = MeasureSpec.GetSize(widthMeasureSpec);
                if (Drawable.IntrinsicWidth > 0)
                {
                    int height = width * Drawable.IntrinsicHeight / Drawable.IntrinsicWidth;
                    SetMeasuredDimension(width, height);
                }
                else
                {
                    base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
                }
            }
            else
            {
                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            }
        }
    }
}
