using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace ver1
{
    /// <summary>
    /// General conditions about the system:
    ///     1- the word must be horizontal (or semi-horeizontal)
    ///     2- no intersection between letters.
    /// 
    /// </summary>
    class Determination
    {
        private UnsafeBitmap word;
        private const Int16 threshold = 9;  // 9 for hand  // 30 from Hassan (OLD)
        private List<UnsafeBitmap> parts = new List<UnsafeBitmap>();
        private string path = @"e:\letters\test11\";
        public Determination(UnsafeBitmap word)
        {
            this.word = word;
            this.word.LockBitmap();
        }

        private UnsafeBitmap createPic(int f, int t, UnsafeBitmap source)
        {
            UnsafeBitmap temp = new UnsafeBitmap(t - f + 1, source.Height);
            temp.LockBitmap();
            for (int x = 0; x < temp.Width; x++)
                for (int y = 0; y < temp.Height; y++)
                    temp.SetPixel(x, y, source.GetPixel(x + f, y));
            return temp;
        }

        public Bitmap determine()
        {
            PixelData readC; bool changed = false;
            UnsafeBitmap temp=null ;
            //////////    REMOVING GRAY COLORS..
            for (int x = 0; x < word.Width; x++)
                for (int y = 0; y < word.Height; y++)
                {
                    readC = word.GetPixel(x, y);
                    if (readC.red != 255 && readC.red != 0)
                    {
                        changed = true;
                        goto fin;
                    }
                }
            fin:
            if (changed)
            {
                temp = new UnsafeBitmap(word.Width, word.Height);
                temp.LockBitmap();
                for (int x = 0; x < word.Width; x++)
                    for (int y = 0; y < word.Height; y++)
                    {
                        readC = word.GetPixel(x, y);
                        if (readC.red > 128)
                            temp.SetPixel(x, y, PixelData.WHITE);
                        else
                            temp.SetPixel(x, y, PixelData.BLACK);
                    }
            }
            //////////     END OF REMOVING GRAY COLORS..
            ///

            if (!changed)
                temp = word;
            //////////     EDGES OF THE WORD
            int startX = 0, endX = temp.Width, startY = 0, endY = temp.Height;
            for (int x = 0; x < temp.Width; x++)
                for (int y = 0; y < temp.Height; y++)
                {
                    readC = temp.GetPixel(x, y);
                    if (readC.red != 255)
                    {
                        startX = x;
                        goto eX;
                    }
                }
            eX:
                for(int x=temp.Width-1;x>=0;x--)
                    for (int y = 0; y < temp.Height; y++)
                    {
                        readC = temp.GetPixel(x, y);
                        if (readC.red != 255)
                        {
                            endX = x;
                            goto sY;
                        }
                    }
            sY:
                for(int y=0;y<temp.Height;y++)
                    for (int x = 0; x < temp.Width; x++)
                    {
                        readC = temp.GetPixel(x, y);
                        if (readC.red != 255)
                        {
                            startY = y;
                            goto eY;
                        }
                    }
            eY:
                for (int y = temp.Height-1; y >= 0; y--)
                    for (int x = 0; x < temp.Width; x++)
                    {
                        readC = temp.GetPixel(x, y);
                        if (readC.red != 255)
                        {
                            endY = y;
                            goto ter;
                        }
                    }
            ter:
            int wi = endX - startX + 1, hei = endY - startY + 1;
            UnsafeBitmap newTemp = new UnsafeBitmap(wi, hei);
            newTemp.LockBitmap();
            for (int x = startX; x <= endX; x++)
                for (int y = startY; y <= endY; y++)
                    newTemp.SetPixel(x-startX, y-startY, temp.GetPixel(x, y));
            temp = newTemp;
            //////////     END EDGES OF THE WORD
            ///
            //////////     SPLITING THE WORD....
            bool cut = true, stop = false;
            int from = 0, to = temp.Width;
            for (int x = 0; x < temp.Width; x++)
            {
                cut = true;
                for (int y = 0; y < temp.Height; y++)
                {
                    readC = temp.GetPixel(x, y);
                    if (readC.red != 255)
                    {
                        cut = false;
                        break;
                    }
                }
                if (cut)
                {
                    to = x - 1;
                    parts.Add(createPic(from, to, temp));
                    for (int x2 = x; x2 < temp.Width; x2++)
                    {
                        stop = false;
                        for (int y2 = 0; y2 < temp.Height; y2++)
                        {
                            if (temp.GetPixel(x2, y2).red != 255)
                            {
                                stop = true;
                                break;
                            }
                        }
                        if (stop)
                        {
                            from = x2;
                            x = x2;
                            break;
                        }
                    }
                }
            }
            parts.Add(createPic(from, temp.Width-1, temp));
            
            //////////     END OF SPLITING THE WORD....
            /////
            //////////     SHARPING EVERY PART
            from = -1; to = -1;  UnsafeBitmap local;
            for (int kk = 0; kk < parts.Count; kk++)
            {
                for (int y = 0; y < parts[kk].Height; y++)
                {
                    for (int x = 0; x < parts[kk].Width; x++)
                    {
                        if (parts[kk].GetPixel(x, y).red != 255)
                        {
                            from = y;
                            goto down;
                        }
                    }
                }
            down:
                for (int y = parts[kk].Height - 1; y >= 0; y--)
                {
                    for (int x = 0; x < parts[kk].Width; x++)
                    {
                        if (parts[kk].GetPixel(x, y).red != 255)
                        {
                            to = y;
                            goto down2;
                        }
                    }
                }
            down2:
                local = new UnsafeBitmap(parts[kk].Width,  to-from + 1);
                local.LockBitmap();
                for (int x = 0; x < parts[kk].Width; x++)
                    for (int y = from; y <= to; y++)
                        local.SetPixel(x, y - from, parts[kk].GetPixel(x, y));
                parts[kk] = local;
            }
            //////////     END OF SHARPING EVERY PART....
            /////
            List<range> tt; int r = 0;
            UnsafeBitmap toSave2 = null;
            for (int j = 0; j < parts.Count; j++)
            {
                parts[j].Bitmap.Save(@path+"par" + j + ".bmp");
//                tt = getRanges(parts[j]);
                tt=lettersConnections(parts[j]);
                int mid = -1, oldMid = -1, begin = -1; UnsafeBitmap toSave = null;
                for (int a = 0; a < tt.Count; a++)
                {
                    for (int g = tt[a].start; g <= tt[a].end; g++)
                        parts[j].SetPixel(g, 0, PixelData.RED);
                    if (tt[a].start == 0 || tt[a].start == 1 )
                        continue;
                    if (tt[a].end == parts[j].Width-1 || tt[a].end == parts[j].Width - 2)
                        continue;
                    oldMid = mid;
                    mid = (tt[a].start + tt[a].end) / 2;
                    if (oldMid != -1)
                    {
//                        if(a==0)
                            begin = oldMid;
  //                      else
    //                        begin=tt[a-1].
                    }
                    else begin = 0;
    //                toSave = new UnsafeBitmap(mid - begin, parts[j].Height);
                    toSave = createPic(begin, mid, parts[j]);
//                    toSave.LockBitmap();
  //                  copyPart(parts[j], toSave, begin, mid);
                    toSave.Bitmap.Save(@path+"LETT" + r + ".bmp");
                    toSave.Dispose();
                    r++;
                }
//                toSave.Dispose();
  //              toSave = null;
/*                toSave2 = new UnsafeBitmap(parts[j].Width - mid-1, parts[j].Height);
                toSave2.LockBitmap();
                copyPart(parts[j], toSave2, mid, parts[j].Width - 1);*/
                //toSave.UnlockBitmap();
                if (mid != -1)
                {
                    toSave2 = createPic(mid, parts[j].Width - 1, parts[j]);
                    toSave2.Bitmap.Save(@path + "LETTo" + r + ".bmp");
                    r++;
                }
                System.GC.Collect();
                parts[j].Bitmap.Save(@path + "line" + j + ".bmp");///////
            }
            temp.UnlockBitmap();
            return temp.Bitmap;
        }

        private void copyPart(UnsafeBitmap from, UnsafeBitmap to, int st, int end)
        {
            for (int x = st; x <= end; x++)
                for (int y = 0; y < from.Height; y++)
                    to.SetPixel(x, y, from.GetPixel(x, y));
        }

        private struct range
        {
            public int start, end;
            public range(int s, int e)
            {
                start = s; end = e;
            }
        }

        private List<range> getRanges(Bitmap pic)  //  Think about spliting over Y (to remove points)==> unpointed letters..
        {
            bool dotFound = false, pass = false, searchForStart = true;
            int st = -1, en = -1; List<range> temp=new List<range>();  // dont forget the last pixel problem.
            for (int x = pic.Width - 1; x >= 0; x--)                   //   DONE>>...
            {
                dotFound = pass = false;
                for (int y = 0; y < pic.Height; y++)
                {
                    if (pic.GetPixel(x, y).R != 255)
                    {
                        if (dotFound/* && pic.GetPixel(x, y - 1).R == 255*/)
                        {
                            pass = true;
                            break;
                        }
                        else
                            dotFound = true;
                    }
                }
                if (!pass && searchForStart && dotFound)
                {
                    st = x;
                    searchForStart = false;
                }
                else if (pass && !searchForStart)
                {
                    en = x + 1;
                    searchForStart = true;
                    temp.Add(new range(st, en));
                }
            }
            if (!searchForStart)
            {
                en = 0;
                temp.Add(new range(st, en));
            }
            return temp;
        }

        private List<range> lettersConnections(UnsafeBitmap part)
        {
            byte startColor = 10; bool inside = false, enterd = false;
            Int16 blackPxiels = 0; int start = -1;
            bool fine = true, lookingForEadge = true ;
            List<range> divided = new List<range>();
            for (int x = 0; x < part.Width; x++)
            {
                startColor = part.GetPixel(x, 0).red;
                blackPxiels = 0;
                fine = true;
                if (startColor == 0)
                {
                    inside = true; enterd = true;
                }
                else { inside = false; enterd = false; }
                for (int y = 1; y < part.Height; y++)
                {
                    if (blackPxiels > threshold)
                    {
                        fine = false;
                        break;
                    }
                    //////////
                    if (part.GetPixel(x, y).red == 0)
                    {
                        if (!inside && !enterd)
                        {
                            blackPxiels++;
                            enterd = true;
                            inside = true;
                            continue;
                        }
                        else if (!inside && enterd)
                        {
                            fine = false;
                            break;
                        }
                        else if (inside && enterd)
                        {
                            blackPxiels++;
                            continue;
                        }
                    }
                    else
                    {
                        if (!inside && !enterd)
                            continue;
                        else if (!inside && enterd)
                            continue;
                        else if (inside && enterd)
                        {
                            inside = false;
                            continue;
                        }
                    }
                }
                if (fine)
                {
                    if (lookingForEadge)
                    {
                        lookingForEadge = false;
                        start = x;
                    }
                }
                else
                {
                    if (!lookingForEadge)
                    {
                        lookingForEadge = true;
                        divided.Add(new range(start, x - 2));
                    }
                }
            }
            return divided;
            /*
             * now sharp the edges:
             * 1- dont consider the range when it's on the sides   <-  or ->
             * 
             */
        }


    }
}
