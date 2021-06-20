using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Bot_Discord_Jeu_de_Carte.Modules
{
    class Image
    {
        public static System.Drawing.Bitmap CombineBitmapL(string[] files)
        {
            //read all images into memory
            List<System.Drawing.Bitmap> images = new List<System.Drawing.Bitmap>();
            System.Drawing.Bitmap finalImage = null;
            try
            {
                int width = 0;
                int height = 0;
                foreach (string image in files)
                {
                    //create a Bitmap from the file and add it to the list
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(image);
                    //update the size of the final bitmap
                    width += bitmap.Width + 100;
                    height = bitmap.Height > height ? bitmap.Height : height;
                    images.Add(bitmap);
                }
                //create a bitmap to hold the combined image
                finalImage = new System.Drawing.Bitmap(width, height);
                //get a graphics object from the image so we can draw on it
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(finalImage))
                {
                    //set background color
                    g.Clear(System.Drawing.Color.FromArgb(47, 49, 54));
                    //go through each image and draw it on the final image
                    int offset = 0;
                    foreach (System.Drawing.Bitmap image in images)
                    {
                        g.DrawImage(image,
                        new System.Drawing.Rectangle(offset, 0, image.Width, image.Height));
                        offset += image.Width + 100;
                    }
                }
                return finalImage;
            }
            catch (Exception)
            {
                if (finalImage != null)
                    finalImage.Dispose();
                //throw ex;
                throw;
            }
            finally
            {
                //clean up memory
                foreach (System.Drawing.Bitmap image in images)
                {
                    image.Dispose();
                }
            }
        }

        public static System.Drawing.Bitmap CombineBitmapH(string[] files)
        {
            //read all images into memory
            List<System.Drawing.Bitmap> images = new List<System.Drawing.Bitmap>();
            System.Drawing.Bitmap finalImage = null;
            try
            {
                int width = 0;
                int height = 0;
                foreach (string image in files)
                {
                    //create a Bitmap from the file and add it to the list
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(image);
                    //update the size of the final bitmap
                    height += bitmap.Height + 100;
                    width = bitmap.Width > width ? bitmap.Width : width;
                    images.Add(bitmap);
                }
                //create a bitmap to hold the combined image
                finalImage = new System.Drawing.Bitmap(width, height);
                //get a graphics object from the image so we can draw on it
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(finalImage))
                {
                    //set background color
                    g.Clear(System.Drawing.Color.FromArgb(47, 49, 54));
                    //go through each image and draw it on the final image
                    int offset = 0;
                    foreach (System.Drawing.Bitmap image in images)
                    {
                        g.DrawImage(image,
                        new System.Drawing.Rectangle(0, offset, image.Width, image.Height));
                        offset += image.Height + 100;
                    }
                }
                return finalImage;
            }
            catch (Exception)
            {
                if (finalImage != null)
                    finalImage.Dispose();
                //throw ex;
                throw;
            }
            finally
            {
                //clean up memory
                foreach (System.Drawing.Bitmap image in images)
                {
                    image.Dispose();
                }
            }
        }
    }
}
