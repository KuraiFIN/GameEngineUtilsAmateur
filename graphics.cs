// This file is so bad but quite impressively colors polygons to the screen on the cpu!
// It's messy and I made it a while back but I'm proud of it

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CSL_Engine.Theta.World
{
    class bresen
    {
        public Bitmap output;
        public int x;
        public int y;
        public float x1;
        public float y1;
        public float x2;
        public float y2;
        public float dx;
        public float dy;
        public float m;
        public float c;
        public float xend;
        void get()
        {
            //cal
            //print("Enter start & end points");
            //print("enter x1, y1, x2, y2");
            //scanf("%f%f%f%f", sx1, sx2, sx3, sx4)
            cal();
        }
        
        public void cal()
        {
            dx = x2 - x1;
            dy = y2 - y1;//2y1;
            m = dy / dx;
            c = y1 - (m * x1);
            if (dx < 0)
            {
                x = (int)x2;
                y = (int)y2;
                xend = x1;
            }
            else
            {
                x = (int)x1;
                y = (int)y1;
                xend = x2;
            }
            
            while (x <= xend)
            {
                //putpixel(x, y, RED);
                output.SetPixel(x, y, Color.Green);
                System.Diagnostics.Debug.WriteLine("X: " + x + ", Y: " + y);
                y++;
                x++;
                y = (x * x) + (int)c;
            }
        }
        public void main()
        {
            float steps = 0.0f;
            if (dx >= dy)
            {
                steps = dx;
            }
            else
            {
                steps = dy;
            }
            dx = dx / steps;
            dy = dy / steps;
            x = (int)x1;
            y = (int)y1;
            int i = 1;
            while (i <= steps)
            {
                output.SetPixel(x, y, Color.Red);
                x += (int)dx;
                y += (int)dy;
                i = i + 1;
            }
            
        }
    }

    class pixel_line {
        //ONLY WORKS IF V2.Y IS GREATER THAN V1.Y
        Bitmap output;
        vector point1;
        vector point2;
        float deltaX;
        float deltaY;
        float d;

        int x;
        int y;

        public pixel_line(vector v1, vector v2, Bitmap o) {

            output = o;
            point1 = v1;
            point2 = v2;

            deltaX = v2.x - v1.x;
            deltaY = v2.y - v1.y;
            d = deltaY * 2 - deltaX;
            float deltaE = deltaY * 2;
            float deltaNE = (deltaY - deltaX) * 2;
            x = (int)v1.x;
            y = (int)v1.y;
            //illuminate X, Y
            output.SetPixel(x, y, Color.Blue);

            /*
            while (x < v2.x) {

                if (d <= 0) {

                    //add deltaE to d
                    d += deltaE;
                    //add 1 to X
                    x += 1;
                }
                else {

                    //add deltaNE to d
                    d += deltaNE;
                    //add 1 to X
                    x += 1;
                    //add 1 to Y
                    y += 1;
                }
                //illuminate X, Y
                output.SetPixel(x, y, Color.Blue);
                
            }
            */
        }
        public void default_direction() {
            //Requirement: x1 < x2 && y1 < y2: produces horizontal => negative 45
            deltaX = point2.x - point1.x;
            deltaY = point2.y - point1.y;
            d = deltaY * 2 - deltaX;
            float deltaE = deltaY * 2;
            float deltaNE = (deltaY - deltaX) * 2;
            x = (int)point1.x;
            y = (int)point1.y;
            //illuminate X, Y
            output.SetPixel(x, y, Color.Purple);

            while (x < point2.x) {
                if (d <= 0) {
                    //add deltaE to d
                    d += deltaE;
                    //add 1 to X
                    x += 1;
                }
                else {
                    //add deltaNE to d
                    d += deltaNE;
                    //add 1 to X
                    x += 1;
                    //add 1 to Y
                    y += 1;
                }
                //illuminate X, Y
                output.SetPixel(x, y, Color.Purple);
            }
        }
        public void positive_direction()
        {
            //Requirement: x1 < x2 && y1 > y2: produces horizontal => positive 45
            deltaX = point2.x - point1.x;
            deltaY = point1.y - point2.y;
            d = deltaY * 2 - deltaX;
            float deltaE = deltaY * 2;
            float deltaNE = (deltaY - deltaX) * 2;
            x = (int)point1.x;
            y = (int)point1.y;
            output.SetPixel(x, y, Color.Purple);

            int differenceY = (int)point1.y - (int)point2.y;
            int addY = 0;
            while (x < point2.x)
            {
                if (d <= 0)
                {
                    d += deltaE;
                    x += 1;
                }
                else
                {
                    d += deltaNE;
                    x += 1;
                    //y += 1;
                    addY += 1;
                }
                output.SetPixel(x, y - addY, Color.Purple);
            }
        }
    }
    public static class PixelPolygon
    {
        //More official implementation of pixel_polygon
        public static Bitmap output;
        public static Size dimensions;
        public static Size halfdimensions;

        public static Color color;
        public static Dictionary<int, KeyValuePair<int, int>> intersections = new Dictionary<int, KeyValuePair<int, int>> { };
        public static Dictionary<int, float> depth = new Dictionary<int, float> { };
        public static KeyValuePair<int, int> keyvalue = default(KeyValuePair<int, int>);
        public static void Create(Bitmap o)
        {
            output = o;
            dimensions = new Size(o.Width, o.Height);
            halfdimensions = new Size(o.Width / 2, o.Height / 2);
            keyvalue = new KeyValuePair<int, int>(-dimensions.Width - 1, dimensions.Width + 1);
            //depth can be accessed like this: depth(x,y)=>depth[y * width + x];
        }
        public static void ColorPolygonB(params vector[] points)
        {
            intersections.Clear();
            vector lowerBounds = points[0];
            vector upperBounds = points[0];
            vector mean = vector.mean(points);
            for (int i = 1; i < points.Length; i++)
            {
                vector point = points[i];
                if (point.x < lowerBounds.x) lowerBounds.x = point.x;
                else if (point.x > upperBounds.x) upperBounds.x = point.x;
                if (point.y < lowerBounds.y) lowerBounds.y = point.y;
                else if (point.y > upperBounds.y) upperBounds.y = point.y;
                if (point.z < lowerBounds.z) lowerBounds.z = point.z;
                else if (point.z > upperBounds.z) upperBounds.z = point.z;
            }
            for (int p = 0; p < points.Length; p++)
            {
                vector point1 = points[p];
                vector point2 = p == points.Length - 1 ? points[0] : points[p + 1];
                float dx = point2.x - point1.x;
                float dy = point2.y - point1.y;
                float gradient = dy != 0 ? dx / dy : dx;//0.0f;

                float ey = (int)(point1.y + 1) - point1.y;
                float ex = gradient * ey;

                // X and Y data for the edge's vector's A and B point
                float line1x = point1.x < point2.x ? point1.x + ex : point2.x + ex;
                float line1y = (int)(point1.y + 1);
                float line2x = point1.x < point2.x ? point2.x + ex : point1.x + ex;
                float line2y = (int)point2.y;

                float buffer_x = point1.x < point2.x ? line2x : line1x;
                if (point2.y > point1.y) buffer_x = point1.x; //IMPORTANT: THIS WAS VITAL IN ONE CONTEXT, IDK ABOUT ANY OTHERS LOL

                int startY = (int)lowerBounds.y;//(int)line1y;
                if (startY < -halfdimensions.Height) startY = -halfdimensions.Height;
                int endY = (int)upperBounds.y;//line2y;
                if (endY > halfdimensions.Height) endY = halfdimensions.Height;
                int lineLowerY = point1.y < point2.y ? (int)point1.y : (int)point2.y;
                int lineUpperY = point1.y > point2.y ? (int)point1.y : (int)point2.y;
                //lineLowerY += 1;
                lineUpperY -= 1;//* (dimensions.Width / 500);
                for (int y = startY - 1; y < endY + 1; y++)
                {
                    if (y < lineLowerY || y > lineUpperY) continue;
                    int yVal = y;// - 1;//+ 1;
                    int b = (int)buffer_x - 1;
                    KeyValuePair<int, int> startpair = new KeyValuePair<int, int>(b, b);
                    KeyValuePair <int, int> intersection = intersections.ContainsKey(y) ? intersections[y] : startpair;
                    if (intersection.Equals(startpair)) intersection = startpair;


                    else
                    {
                        bool lowerThanStart = b < intersection.Key || intersection.Key == keyvalue.Key;
                        bool endRewriteOkay = intersection.Value == keyvalue.Value || intersection.Value == keyvalue.Key;
                        bool alone = lowerThanStart && endRewriteOkay && intersection.Key != keyvalue.Key;//
                        if (lowerThanStart && endRewriteOkay)
                        {
                            
                            intersection = new KeyValuePair<int, int>(b, intersection.Key);
                        }
                        else if (lowerThanStart) intersection = new KeyValuePair<int, int>(b, intersection.Value);

                        else if (b > intersection.Value || intersection.Value == startpair.Value)
                        {
                            intersection = new KeyValuePair<int, int>(intersection.Key, b);
                        }
                    }
                    intersections[y] = intersection;
                    //System.Diagnostics.Debug.WriteLine(">>>> " + intersection);
                    buffer_x = buffer_x + gradient;
                }
            }
            int lowerY = (int)lowerBounds.y;
            if (lowerY < -halfdimensions.Height) lowerY = -halfdimensions.Height;
            int upperY = (int)upperBounds.y;
            if (upperY > halfdimensions.Height) upperY = halfdimensions.Height;
            //System.Diagnostics.Debug.WriteLine("made it");
            if (upperY < -dimensions.Height || lowerY < -dimensions.Height || upperY > dimensions.Height || lowerY > dimensions.Height) return;
            for (int i = lowerY + 1; i < upperY - 1; i++)
            {
                KeyValuePair<int, int> intersection = intersections.ContainsKey(i) ? intersections[i] : keyvalue;
                if (intersection.Key < -dimensions.Width || intersection.Key > dimensions.Width) return;
                if (!intersections.ContainsKey(i) || Math.Abs(intersection.Value) >= halfdimensions.Width - 1 || Math.Abs(intersection.Key) >= halfdimensions.Width - 1) continue;
                //string solo = intersection.Value == keyvalue.Key ? " single " : string.Empty;
                //System.Diagnostics.Debug.WriteLine("For " + solo + "Y = " + i + ": xStart = " + xStart[i] + " and xEnd = " + xEnd[i]);

                if (intersection.Value == keyvalue.Key)
                {
                    // Centre
                    int TrueX = halfdimensions.Width + intersection.Key;
                    int TrueY = halfdimensions.Height - i;
                    if (TrueX < 0 || TrueX >= output.Width || TrueY < 0 || TrueY >= output.Height) continue;
                    bool containsZ = depth.ContainsKey(TrueY * dimensions.Width + TrueX);
                    //if (!containsZ) output.SetPixel(TrueX, TrueY, color);
                    if (!containsZ || depth[TrueY * dimensions.Width + TrueX] < mean.z)
                    {
                        output.SetPixel(TrueX, TrueY, color);
                        depth[TrueY * dimensions.Width + TrueX] = mean.z;
                    }
                    continue;
                }
                int startX = intersection.Key;
                if (startX < -halfdimensions.Width) continue;//startX = -halfdimensions.Width;
                int endX = intersection.Value;
                if (endX > halfdimensions.Width) continue;//endX = halfdimensions.Width;
                for (int n = startX - 1; n < endX + 1; n++)
                {
                    int TrueX = halfdimensions.Width + n;
                    int TrueY = halfdimensions.Height - i;
                    if (TrueX < 0 || TrueX >= output.Width || TrueY < 0 || TrueY >= output.Height) break;
                    bool containsZ = depth.ContainsKey(TrueY * dimensions.Width + TrueX);
                    //if (!containsZ || depth[TrueY * dimensions.Width + TrueX] > mean.z) output.SetPixel(TrueX, TrueY, color);
                    if (!containsZ || depth[TrueY * dimensions.Width + TrueX] < mean.z)
                    {
                        output.SetPixel(TrueX, TrueY, color);
                        depth[TrueY * dimensions.Width + TrueX] = mean.z;
                    }
                }
            }

        }

    }


    public static class pixel_polygon {
        public static Bitmap output;
        public static Color color;
        public static vector[] coords;
        public static void ColorPolygonB(Bitmap o, params vector[] points)
        {
            output = o;
            ColorPolygonB(points);
        }
        public static void ColorPolygonB(params vector[] points)
        {
            // This code is kinda scatterbrained because I only understand what I do some of the time and
            // the scan converting pixels isn't a great conversation starter on a first date

            //Anti-aliasing gist:
            //percent_x = 1 - abs(x - roundedx)
            //percent_y = 1 - abs(y - roundedy)
            //percent = percent_x * percent_y
            //draw_pixel(coordinates = (roundedx, roundedy), color = percent(range 0 - 1))

            //vector center = vector.zero;
            int leftMost = 0;
            int rightMost = 0;
            int upperMost = 0;
            int lowerMost = 0;
            coords = new vector[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                vector pointA = points[i];
                //center += pointA;
                coords[i] = pointA;
                if (pointA.x < points[leftMost].x) leftMost = i;
                else if (pointA.x > points[rightMost].x) rightMost = i;
                if (pointA.y < points[lowerMost].y) lowerMost = i;
                else if (pointA.y > points[upperMost].y) upperMost = i;
            }
            //center /= points.Length;
            //if (center.z < 0) return;

            KeyValuePair<int, int>[] xStart = new KeyValuePair<int, int>[(int)(points[upperMost].y - points[lowerMost].y) + 1];//new int[Math.Abs((int)(boundsUpper.y - boundsLower.y))];
            KeyValuePair<int, int>[] xEnd = new KeyValuePair<int, int>[xStart.Length];
            KeyValuePair<int, int> def = default(KeyValuePair<int, int>);

            for (int i = 0; i < points.Length; i++)
            {
                vector point1 = points[i];
                vector point2 = i >= points.Length - 1 ? points[0] : points[i + 1];
                //System.Diagnostics.Debug.WriteLine("?!?!? " + point1.ToString() + ", " + point2.ToString());
                bool left = point1.x < point2.x;//
                bool positive_slope = point1.x < point2.x && point2.y > point1.y;
                bool negative_slope = point1.x < point2.x && point1.y > point2.y;
                bool positive_slopeB = point2.x < point1.x && point1.y > point2.y;
                bool negative_slopeB = point2.x < point1.x && point2.y > point1.y;

                float dx = point2.x - point1.x;
                // Pixels along y
                float dy = point2.y - point1.y;
                //float dy2 = (float)xStart.Length;//
                float gradient = dy != 0 ? dx / dy : dx;//0.0f;

                float ey = (int)(point1.y + 1) - point1.y;
                float ex = gradient * ey;

                // X and Y data for the edge's vector's A and B point
                float line1x = point1.x < point2.x ? point1.x + ex : point2.x + ex;
                float line1y = (int)(point1.y + 1);
                float line2x = point1.x < point2.x ? point2.x + ex : point1.x + ex;//0.0f;
                //line2x = -17.0f;
                float line2y = (int)point2.y;


                //loop
                int index = 0;

                bool leftToRight = point1.x < point2.x;
                bool bottomToTop = point1.y < point2.y;
                float buffer_x = leftToRight ? line2x : line1x;
                if (point2.y > point1.y) buffer_x = point1.x; //IMPORTANT: THIS WAS VITAL IN ONE CONTEXT, IDK ABOUT ANY OTHERS LOL
                if (!leftToRight)
                {
                    //gradient = dy != 0 ? (point1.x - point2.x) / dy : (point1.x - point2.x);
                    //ex = gradient * ey;
                    //line1x = point2.x + ex;
                    //line2x = point1.x + ex;
                    //buffer_x = (int)line2x;
                    //if enabled, disable - on fixed_gradient bitwise
                }
                if ((gradient > 0 && leftToRight) || (gradient < 0 && !leftToRight)) { }
                else
                {
                    //gradient *= -1;

                }

                int startY = (int)points[lowerMost].y;//(int)line1y;
                int endY = (int)points[upperMost].y;//line2y;
                int lineLowerY = point1.y < point2.y ? (int)point1.y : (int)point2.y;
                int lineUpperY = point1.y > point2.y ? (int)point1.y : (int)point2.y;
                for (int n = startY; n < endY; n++)
                {
                    if (n < lineLowerY || n > lineUpperY) continue;
                    if (index < 0 || index >= xStart.Length)
                    {
                        continue;
                        //TODO: initialize xStart better
                    }
                    int Y = n;
                    int yVal = Y + 1;//point2.y < point1.y && point1.x < point2.x ? endY - Y : Y;
                    int b = (int)buffer_x;
                    if (xStart[Y - startY].Equals(def)) xStart[Y - startY] = new KeyValuePair<int, int>(b, yVal);

                    else
                    {
                        bool lowerThanStart = b < xStart[Y - startY].Key;
                        bool endRewriteOkay = xEnd[Y - startY].Equals(def) || xStart[Y - startY].Key > xEnd[Y - startY].Key;
                        if (lowerThanStart && endRewriteOkay)
                        {
                            xEnd[Y - startY] = xStart[Y - startY];
                            xStart[Y - startY] = new KeyValuePair<int, int>(b, yVal);
                        }
                        else if (lowerThanStart) xStart[Y - startY] = new KeyValuePair<int, int>(b, yVal);

                        else if (xEnd[Y - startY].Equals(def))
                        {
                            xEnd[Y - startY] = new KeyValuePair<int, int>(b, yVal);
                        }
                        else if (b > xEnd[Y - startY].Key) xEnd[Y - startY] = new KeyValuePair<int, int>(b, yVal);
                    }
                    //xStart[Y - startY] = (int)buffer_x;
                    //System.Diagnostics.Debug.WriteLine("X: " + buffer_x + ", Y: " + Y);

                    float fixed_gradient = leftToRight ? gradient : gradient;
                    //if (Y <= lineUpperY && Y >= lineLowerY)
                        buffer_x = buffer_x + fixed_gradient;


                    index += 1; //n but starts at 0
                }


                //int height = 0;
            }
            for (int i = 1; i < xStart.Length; i++)
            {
                string solo = xEnd[i].Equals(def) ? " single " : string.Empty;
                //System.Diagnostics.Debug.WriteLine("For " + solo + "Y = " + i + ": xStart = " + xStart[i] + " and xEnd = " + xEnd[i]);

                if (xEnd[i].Equals(def))
                {
                    // Centre
                    int TrueX = output.Width / 2 + xStart[i].Key;
                    int TrueY = output.Height / 2 - xStart[i].Value;
                    if (TrueX < 0 || TrueX >= output.Width || TrueY < 0 || TrueY >= output.Height) continue;
                    output.SetPixel(TrueX, TrueY, color);
                    continue;
                }
                for (int n = xStart[i].Key; n < xEnd[i].Key; n++)
                {
                    int TrueX = output.Width / 2 + n;
                    int TrueY = output.Height / 2 - xStart[i].Value;
                    if (TrueX < 0 || TrueX >= output.Width || TrueY < 0 || TrueY >= output.Height) continue;
                    output.SetPixel(TrueX, TrueY, color);
                }
            }
        }
        public static void ColorPolygon(Bitmap o, params vector[] points)
        {
            //points = new vector[] { new vector(1.1f, 4.9f), new vector(3.3f, 1.1f), new vector(6.4f, 6.7f)};
            int leftMost = 0;
            int rightMost = 0;
            int upperMost = 0;
            int lowerMost = 0;
            output = o;
            coords = new vector[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                vector pointA = points[i];
                coords[i] = pointA;
                if (pointA.x < points[leftMost].x) leftMost = i;
                else if (pointA.x > points[rightMost].x) rightMost = i;
                if (pointA.y < points[lowerMost].y) lowerMost = i;
                else if (pointA.y > points[upperMost].y) upperMost = i;
            }

            KeyValuePair<int, int>[] xStart = new KeyValuePair<int, int>[(int)(points[upperMost].y - points[lowerMost].y) + 1];//new int[Math.Abs((int)(boundsUpper.y - boundsLower.y))];
            KeyValuePair<int, int>[] xEnd = new KeyValuePair<int, int>[xStart.Length];
            KeyValuePair<int, int> def = default(KeyValuePair<int, int>);

            for (int i = 0; i < points.Length; i++)
            {
                vector point1 = points[i];
                vector point2 = i >= points.Length - 1 ? points[0] : points[i + 1];
                System.Diagnostics.Debug.WriteLine("?!?!? " + point1.ToString() + ", " + point2.ToString());
                bool left = point1.x < point2.x;//
                bool positive_slope = point1.x < point2.x && point2.y > point1.y;
                bool negative_slope = point1.x < point2.x && point1.y > point2.y;
                bool positive_slopeB = point2.x < point1.x && point1.y > point2.y;
                bool negative_slopeB = point2.x < point1.x && point2.y > point1.y;
                
                float dx = point2.x - point1.x;
                // Pixels along y
                float dy = point2.y - point1.y;
                float gradient = dy != 0 ? dx / dy : dx;//0.0f;
                
                float ey = (int)(point1.y + 1) - point1.y;
                float ex = gradient * ey;

                // X and Y data for the edge's vector's A and B point
                float line1x = point1.x + ex;
                float line1y = (int)(point1.y + 1);
                float line2x = 0.0f;
                float line2y = (int)point2.y;


                //loop
                int index = 0;
                
                bool leftToRight = point1.x < point2.x;
                float buffer_x = leftToRight ? line1x : line2x;
                if (!leftToRight)
                {
                    //gradient = dy != 0 ? (point1.x - point2.x) / dy : (point1.x - point2.x);
                    //ex = gradient * ey;
                    //line1x = point2.x + ex;
                    //line2x = point1.x + ex;
                    //buffer_x = (int)line2x;
                    //if enabled, disable - on fixed_gradient bitwise
                }
                if ((gradient > 0 && leftToRight) || (gradient < 0 && !leftToRight)) { }
                else
                {
                    gradient *= -1;
                    
                }

                int startY = (int)points[lowerMost].y;//(int)line1y;
                int endY = (int)points[upperMost].y;//line2y;
                for (int n = startY; n < line2y; n++)
                {
                    if (index < 0 || index >= xStart.Length)
                    {
                        continue;
                        //TODO: initialize xStart better
                    }
                    int Y = n;
                    int b = (int)buffer_x;
                    if (xStart[Y - startY].Equals(def)) xStart[Y - startY] = new KeyValuePair<int, int>(b, Y + 1);

                    else
                    {
                        bool lowerThanStart = b < xStart[Y - startY].Key;
                        bool endRewriteOkay = xEnd[Y - startY].Equals(def) || xStart[Y - startY].Key > xEnd[Y - startY].Key;
                        if (lowerThanStart && endRewriteOkay)
                        {
                            xEnd[Y - startY] = xStart[Y - startY];
                            xStart[Y - startY] = new KeyValuePair<int, int>(b, Y + 1);
                        }
                        else if (lowerThanStart) xStart[Y - startY] = new KeyValuePair<int, int>(b, Y + 1);

                        else if (xEnd[Y - startY].Equals(def))
                        {
                            xEnd[Y - startY] = new KeyValuePair<int, int>(b, Y + 1);
                        }
                        else if (b > xEnd[Y - startY].Key) xEnd[Y - startY] = new KeyValuePair<int, int>(b, Y + 1);
                    }
                    //xStart[Y - startY] = (int)buffer_x;
                    System.Diagnostics.Debug.WriteLine("X: " + buffer_x + ", Y: " + Y);

                    float fixed_gradient = leftToRight ? gradient : gradient;
                    buffer_x = buffer_x + fixed_gradient;


                    index += 1; //n but starts at 0
                }


                //int height = 0;
            }
            for (int i = 1; i < xStart.Length; i++)
            {
                string solo = xEnd[i].Equals(def) ? " single " : string.Empty;
                System.Diagnostics.Debug.WriteLine("For " + solo + "Y = " + i + ": xStart = " + xStart[i] + " and xEnd = " + xEnd[i]);

                if (xEnd[i].Equals(def))
                {
                    // Centre
                    int TrueX = o.Width / 2 + xStart[i].Key;
                    int TrueY = o.Height / 2 - xStart[i].Value;
                    if (TrueX < 0 || TrueX >= o.Width || TrueY < 0 || TrueY >= o.Height) continue;
                    o.SetPixel(TrueX, TrueY, Color.Green);
                    continue;
                }
                for (int n = xStart[i].Key; n < xEnd[i].Key; n++)
                {
                    int TrueX = o.Width / 2 + n;
                    int TrueY = o.Height / 2 - xStart[i].Value;
                    if (TrueX < 0 || TrueX >= o.Width || TrueY < 0 || TrueY >= o.Height) continue;
                    o.SetPixel(TrueX, TrueY, Color.Green);
                }
            }

        }
    }
}
