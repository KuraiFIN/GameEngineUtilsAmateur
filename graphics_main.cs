// The main WinForms file that colors polygons to a bitmap using the CPU!
// Old and cringy but still proud of it

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CSL_Engine.Theta.World;
using CSL_Engine.Theta.World.Experimental;

namespace weaver3D
{
    public partial class Form1 : Form
    {

        int cursor_x = -1;
        int cursor_y = -1;
        bool cursor_moving = false;

        vector previousLocation;

        bool timeToClear = true;
        public Size windowSize;
        Bitmap bm = new Bitmap(800, 400);
        Graphics g;
        //Graphics.DrawPolygon
        Graphics picture_space;
        BufferedGraphics bufferedGraphics;
        BufferedGraphicsContext cntxt = BufferedGraphicsManager.Current;
        Pen pen;


        public hierarchy scene_hierarchy;
        public camera scene_camera;
        transform tpoint;
        transform cube;
        public Armature rig;
        public KinematicChain kc;
        public float cam_distance = 10f;


        bool middle_mouse_click = false;
        bool mouse_moving = false;
        public float rotate_scene_y = 0.0f;
        public float rotate_scene_x = 0.0f;
        public float rotate_scene_y_total = 0f;
        public float rotate_scene_x_total = 0f;

        public Form1()
        {
            InitializeComponent();
            this.pictureBox1.MouseWheel += new MouseEventHandler(this.pictureBox1_MouseWheel);

            AllScripts.form1 = this;
            //Make window hidden and out of sight:
            //this.WindowState = FormWindowState.Minimized;
            //this.ShowInTaskbar = false;

            this.BackColor = Color.LightGray;
            windowSize = new Size(pictureBox1.Width, pictureBox1.Height);
            //this.TransparencyKey = Color.LightGray;
            this.TransparencyKey = Color.LimeGreen;
            g = panel1.CreateGraphics();
            //picture_space = pictureBox1.CreateGraphics();//Graphics.FromImage(bm);
            picture_space = Graphics.FromImage(bm);
            picture_space = pictureBox1.CreateGraphics();
            cntxt.MaximumBuffer = new Size(800, 400);
            bufferedGraphics = cntxt.Allocate(picture_space, new Rectangle(Point.Empty, new Size(800, 400)));
            pen = new Pen(Color.LimeGreen, 4);
            pictureBox1.Image = bm;


            scene_hierarchy = new hierarchy();
            scene_hierarchy.lights = new light[] { new light(vector.zero, vector.backward, 1.0f) };
            scene_camera = new camera(scene_hierarchy, new transform((vector.forward) + vector.downward, quat.identity));
            scene_camera.transform.name = "view";
            //scene_camera.transform.parent = new transform(vector.zero, quat.identity);
            //scene_camera.transform.location_local = vector.forward * 10f;
            transform floorplan = new transform(vector.backward, quat.identity, mesh.create_example_floorplan())
            {
                name = "floorplan",
                modifiers = new modifier[]
                {
                    //new modifier(){type = "add_layer"}
                    modifier.create_add_layer(new float[]{0.1f, 0f , -0.1f, 0f}, vector.upward * 0.1f, vector.upward, vector.upward*0.1f, vector.upward)
                }
            };
            transform arm = transform.example_armature(vector.forward, quat.identity);

            tpoint = new transform(vector.zero, quat.identity, mesh.create_cube(0.1f)) { name = "point" };
            //cube = new transform(vector.backward * 6f, quat.identity, mesh.create_cube()) { modifiers = new modifier[] { new modifier() { type = "make_opening" } } };
            //cube = new transform(vector.backward * 6f, quat.identity, mesh.create_cube()) { modifiers = new modifier[] { new modifier() { type = "align_object", target_transform = tpoint } } };
            cube = new transform(vector.backward * 6f, quat.identity, mesh.create_cube())
            {
                modifiers = new modifier[] {
                    new modifier() { type = "add_layer"},
                    new modifier(){type = "align_object", target_transform = tpoint}
                }
            };
            rig = Armature.CreateBasicHuman(arm, false);
            rig.rest_pose = ArmaturePose.CreateAndCapture(rig, CommonBones.allHuman);
            transform bpy = mesh.load_from_persistent("bpy_object");
            bpy.modifiers = new modifier[] { new modifier() { armature = rig } };
            kc = new KinematicChain() { bones = new string[] { "brachial_L", "antebrachial_L" }, inputWeights = new float[] {0.5f, 1f }, pointTarget = tpoint.location };
            kc.rig = rig;
            //kc.CreatePositions();


            scene_hierarchy.children = new transform[] {
                cube, floorplan, arm, scene_camera.transform, tpoint, bpy
            };
            
            ColorPolygon(new vector[] { new vector(1.1f, 4.9f), new vector(3.3f, 1.1f), new vector(6.4f, 6.7f) });

            scene_hierarchy.children[0].render_buffer_append(scene_camera);
            

            PixelPolygon.Create((Bitmap)pictureBox1.Image);

            scene_camera.update_render_buffer_mesh();
            for (int i = 0; i < scene_camera.buffer_render_mesh.faces.Length; i++)
            {
                //if (i != 5) continue;
                face face = scene_camera.buffer_render_mesh.faces[i];
                vector[] verts = new vector[face.indices.Length];
                for (int n = 0; n < face.indices.Length; n++) verts[n] = scene_camera.buffer_render_obj.rotation * scene_camera.buffer_render_mesh.vertices[face.indices[n]].v;
                pixel_polygon.ColorPolygonB((Bitmap)pictureBox1.Image, verts);
            }

            popup p = new popup();
            p.Activate();
            p.Show(this);
            SceneInfo si = new SceneInfo();
            si.Activate();
            si.Show(this);
            //si.Owner = this;
            //ListView is similar to TreeView but acts like File explorer with details; ListView.SubItems.Add(first call for name, date, etc);
            TreeView t = (TreeView)si.Controls["SceneTree"];
            //TreeView t = (TreeView)Controls.Find("treeView1", false)[0];
            System.Diagnostics.Debug.WriteLine(t);
            t.SelectedNode.Nodes.Add("dumb example", "dumb example");
            t.SelectedNode.Nodes.Add("another dumb example");
            t.Nodes[0].Nodes["dumb example"].Nodes.Add("nested dumb thingy", "nested dumb thingy");
            System.Diagnostics.Debug.WriteLine("??? " + t.Nodes.Find("nested dumb thingy", true)[0]);
            if (AllScripts.sceneInfo != null) AllScripts.sceneInfo.TreeChildrenStart(scene_hierarchy.children);

            UserControl1 userControl1 = new UserControl1();
            userControl1.Location = new Point(25, 25);
            this.Controls.Add(userControl1);
            userControl1.Show();
            userControl1.BringToFront();
            TextBox txtbox = userControl1.Controls.Find("textBox1", false)[0] as TextBox;
            txtbox.Text = "rewritten";

            AddLayerModifierDisplay addLayerModifierDisplay = new AddLayerModifierDisplay();
            addLayerModifierDisplay.Location = new Point(25, 260);
            this.Controls.Add(addLayerModifierDisplay);
            addLayerModifierDisplay.Show();
            addLayerModifierDisplay.BringToFront();
            ModifierDisplayForm mdf = new ModifierDisplayForm();
            mdf.Activate();
            mdf.Show(this);

        }
        private void update_view()
        {
            if (AllScripts.userControl1 != null)
            {
                //TextBox txtbox = AllScripts.userControl1.Controls.Find("textBox1", false)[0] as TextBox;
                //txtbox.Text = timer1.ToString();
            }
            scene_camera.render_scene(pictureBox1, scene_hierarchy.children);
            kc.pointTarget = tpoint.location;
            kc.CreatePositions();

            if (middle_mouse_click && mouse_moving)
            {

                //this.pictureBox1.MouseWheel += new MouseEventHandler(this.pictureBox1_MouseWheel);
                pictureBox1.Focus();
                //System.Diagnostics.Debug.WriteLine(windowSize);
                windowSize = new Size(pictureBox1.Width, pictureBox1.Height);
                scene_camera.windowSize = windowSize;

                //cam_distance = 10.0f;
                scene_camera.transform.location = vector.forward * cam_distance;
                scene_camera.transform.location = scene_camera.transform.rotation * scene_camera.transform.location;


                scene_camera.transform.rotation = quat.CreateFromAxisAngle(vector.upward, -rotate_scene_x_total) * quat.CreateFromAxisAngle(vector.right, -rotate_scene_y_total);

                //scene_camera.transform.parent.rotation = quat.CreateFromAxisAngle(vector.upward, -rotate_scene_x_total) * quat.CreateFromAxisAngle(vector.right, -rotate_scene_y_total);
            }
            mouse_moving = false;
            
        }
        private void pictureBox1_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ///process mouse event
            //System.Diagnostics.Debug.WriteLine(e.Delta);
            float amt = e.Delta > 0 ? 1.2f : -1.2f;
            cam_distance -= amt;
            scene_camera.transform.location = vector.forward * cam_distance;
            scene_camera.transform.location = scene_camera.transform.rotation * scene_camera.transform.location;
            //System.Diagnostics.Debug.WriteLine(cam_distance);
        }
        public void update_ui()
        {

        }

        private void FileOpen()
        {
            using(OpenFileDialog ofd = new OpenFileDialog() { Multiselect = true, ValidateNames = true, Filter = "JPEG|*.jpg" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (string filename in ofd.FileNames)
                    {
                        System.IO.FileInfo fi = new System.IO.FileInfo(filename);
                    }
                }
            }
        }

        private void ColorPolygon(params vector[] points) {

            // Example points
            //points = new vector[] { new vector(1.1f, 4.9f), new vector(3.3f, 1.1f), new vector(6.4f, 6.7f)};

            // Above any possible loops below
            vector center = points[0];
            for (int i = 1; i < points.Length; i++) {
                center += points[i];
            }
            center /= (float) points.Length;
            vector boundsLower = new vector(points[0].x, points[0].y);
            vector boundsUpper = new vector(points[0].x, points[0].y);
            int leftMost = 0;
            int rightMost = 0;
            int upperMost = 0;
            int lowerMost = 0;
            for (int i = 0; i < points.Length; i++) {
                vector pointA = points[i];

                if (pointA.x < points[leftMost].x)
                {
                    leftMost = i;
                    boundsLower.x = pointA.x;
                }
                else if (pointA.x > points[rightMost].x) {
                    rightMost = i;
                    boundsUpper.x = pointA.x;
                }
                if (pointA.y < points[lowerMost].y)
                {
                    lowerMost = i;
                    boundsLower.y = pointA.y;
                }
                else if (pointA.y > points[upperMost].y) {
                    upperMost = i;
                    boundsUpper.y = pointA.y;
                }
            }
            KeyValuePair<int, int>[] xStart = new KeyValuePair<int, int>[(int)(points[upperMost].y - points[lowerMost].y) + 1];//new int[Math.Abs((int)(boundsUpper.y - boundsLower.y))];
            KeyValuePair<int, int>[] xEnd = new KeyValuePair<int, int>[xStart.Length];
            KeyValuePair<int, int> def = default(KeyValuePair<int, int>);

            for (int i = 0; i < points.Length; i++)
            {
                vector point1 = points[i];
                vector point2 = i >= points.Length - 1 ? points[0] : points[i + 1];
                bool left = point1.x < point2.x;//
                bool positive_slope = point1.x < point2.x && point2.y > point1.y;
                bool negative_slope = point1.x < point2.x && point1.y > point2.y;
                bool positive_slopeB = point2.x < point1.x && point1.y > point2.y;
                bool negative_slopeB = point2.x < point1.x && point2.y > point1.y;

                if (negative_slope) { }
                else if (negative_slopeB)
                {
                    // Definitely do for negative_slopeB
                    //point1 = point2;
                    //point2 = points[i];
                }

                // Pixels along x
                float dx = point2.x - point1.x;
                // Pixels along y
                float dy = point2.y - point1.y;
                float gradient = dy != 0 ? dx / dy : dx;//0.0f;
                if (negative_slope ||  negative_slopeB)
                {
                    //dy = point1.y - point2.y;
                    //gradient = dy != 0 ? dx / dy : dx;
                }
                if (positive_slopeB || negative_slopeB)
                {
                    //dx = point1.x - point2.x;
                    //gradient = dy/dx;
                    
                    
                }
                if (negative_slopeB) {

                    //gradient *= -1.0f;
                }
                


                // Represent the midpoint of a pixel at the angle of a polygon
                float ey = (int)(point1.y + 1) - point1.y;
                float ex = gradient * ey;

                // X and Y data for the edge's vector's A and B point
                float line1x = point1.x + ex;
                float line1y = (int)(point1.y + 1);
                float line2x = 0.0f;
                float line2y = (int)point2.y;
                

                //loop
                int index = 0;
                float buffer_x = line1x;
                System.Diagnostics.Debug.WriteLine("!!!!! " + buffer_x);
                System.Diagnostics.Debug.WriteLine((line2y - line1y).ToString());
                System.Diagnostics.Debug.WriteLine("Grad: " + gradient);
                System.Diagnostics.Debug.WriteLine("Buff Size: " + xStart.Length);
                bool ascending = true;
                if (negative_slope || (line2y < line1y && line2x > line1x))
                {
                    // Definitely do for negative slope
                    float tmp = line2y;
                    line2y = line1y;
                    line1y = tmp;
                    ascending = false;
                }

                int startY = (int)points[lowerMost].y;//(int)line1y;
                int endY = (int)line2y;
                for (int n = startY; n <= line2y; n++)
                {
                    if (index >= xStart.Length)
                    {
                        continue;
                        //TODO: initialize xStart better
                    }

                    //loop y from Ay to By

                    //YBuffer(y) = buffer_x;
                    //int Y = negative_slope ? (int)line2y - n + 1  : n - 1; //TODO: inconsistent -1'ing but necessary
                    int Y = negative_slope ? (int)line2y - n : n - startY;
                    Y = n;
                    //if (Y < 0) Y  = (Y * -1); //Stupid line
                    Console.WriteLine("Index: " + index + ", xStart.Length: " + xStart.Length + ", Y: " + Y);
                    //xStart[index] = (int)buffer_x;
                    int b = (int)buffer_x;
                    if (xStart[Y - startY].Equals(def)) xStart[Y - startY] = new KeyValuePair<int, int>(b, Y + 1);
                    
                    else {
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

                    float fixed_gradient = ascending ? gradient : -gradient;
                    //if ( positive_slopeB) fixed_gradient *= 2;
                    buffer_x = buffer_x + fixed_gradient;
                    

                    index += 1; //n but starts at 0
                }
                

                //int height = 0;
            }
            for (int i = 0; i < xStart.Length; i++) {
                string solo = xEnd[i].Equals(def) ? " single " : string.Empty;
                System.Diagnostics.Debug.WriteLine("For " + solo + "Y = " + i + ": xStart = " + xStart[i] + " and xEnd = " + xEnd[i]);

                if (xEnd[i].Equals(def))
                {
                    for (int n = xStart[i].Key; n < xEnd[i].Key; n++)
                    {
                        // Centre
                        int TrueX = pictureBox1.Width / 2 + n;
                        int TrueY = pictureBox1.Height / 2 + xStart[i].Value;
                        if (TrueX < 0 || TrueX >= pictureBox1.Width || TrueY < 0 || TrueY >= pictureBox1.Height) continue;
                        ((Bitmap)pictureBox1.Image).SetPixel(TrueX, TrueY, Color.Red);
                    }
                    continue;
                }
                for (int n = xStart[i].Key; n < xEnd[i].Key; n++)
                {
                    int TrueX = pictureBox1.Width / 2 + n;
                    int TrueY = pictureBox1.Height / 2 + xStart[i].Value;
                    if (TrueX < 0 || TrueX >= pictureBox1.Width || TrueY < 0 || TrueY >= pictureBox1.Height) continue;
                    ((Bitmap)pictureBox1.Image).SetPixel(TrueX, TrueY, Color.Red);
                }
            }
            pictureBox1.Refresh();

        }

        private void render_scene() {

            for (int i = 0; i < scene_camera.buffer_render_mesh.vertices.Length; i++)
            {
                vector vertex = scene_camera.buffer_render_mesh.vertices[i].v;
                vector transformed = new vector(vertex.x/vertex.z, vertex.y/vertex.z, vertex.z);
                //pScreen.x = pCamera.x / -pCamera.z;
                //pScreen.y = pCamera.y / -pCamera.z;

            }
            /*
             * avg_z[0] = [face_index, z_float]; reverse = True: sorts descending
             * tmp = [int face_index, float z] sorted descending
             * f = face corresponding to face_index
             * t = transformed_vertices //can probably just use global location
             * pixel = transformed/projected vert
            

            for v in self.vertices:
                # Rotate the point around X axis, then around Y axis, and finally around Z axis.
                r = v.rotateX(self.angle).rotateY(self.angle).rotateZ(self.angle)
                # Transform the point from 3D to 2D
                p = r.project(self.screen.get_width(), self.screen.get_height(), 256, 4)
                # Put the point in the list of transformed vertices
                t.append(p)

            # Calculate the average Z values of each face.
            avg_z = []
            i = 0
            for f in self.faces:
                z = (t[f[0]].z + t[f[1]].z + t[f[2]].z + t[f[3]].z) / 4.0
                avg_z.append([i,z])
                i = i + 1
            for tmp in sorted(avg_z, key = itemgetter(1), reverse = True):
                face_index = tmp[0]
                f = self.faces[face_index]
                pointlist = [(t[f[0]].x, t[f[0]].y), (t[f[1]].x, t[f[1]].y),
                             (t[f[1]].x, t[f[1]].y), (t[f[2]].x, t[f[2]].y),
                             (t[f[2]].x, t[f[2]].y), (t[f[3]].x, t[f[3]].y),
                             (t[f[3]].x, t[f[3]].y), (t[f[0]].x, t[f[0]].y)]
                pygame.draw.polygon(self.screen, self.colors[face_index], pointlist)
            */
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            mouse_moving = true;
            if (cursor_moving) {
            //g.DrawLine(pen, new Point (cursor_x, cursor_y), e.Location);
            cursor_x = e.X;
            cursor_y = e.Y;
            //System.Diagnostics.Debug.WriteLine(e.Location.ToString());
            }
            //update_view();
            if (e.Button != MouseButtons.Middle) return;
            if (middle_mouse_click && mouse_moving)
            {
                vector l = new vector((float)e.Location.X, (float)e.Location.Y);
                vector difference = l - previousLocation;
                rotate_scene_x = difference.x;
                rotate_scene_y = difference.y;
                rotate_scene_x_total += rotate_scene_x;
                rotate_scene_y_total += rotate_scene_y;
                previousLocation = l;
            }

            /*
            pictureBox1.Image = new Bitmap(800, 400);
            //pictureBox1.Refresh();
            //scene_hierarchy.children[0].rotation_local = quat.CreateFromAxisAngle(vector.upward, rotate_scene_y) * quat.CreateFromAxisAngle(vector.forward, rotate_scene_x);
            //scene_hierarchy.children[0].render_buffer_append(scene_camera);
            scene_camera.buffer_render_obj.rotation_local = quat.CreateFromAxisAngle(vector.upward, rotate_scene_y) * quat.CreateFromAxisAngle(vector.forward, rotate_scene_x);
            for (int i = 0; i < scene_camera.buffer_render_obj.mesh.faces.Length; i++)
            {
                //if (i != 5) continue;
                int[] face = scene_camera.buffer_render_obj.mesh.faces[i];
                vector[] vertices = new vector[face.Length];
                for (int n = 0; n < face.Length; n++) vertices[n] = scene_camera.buffer_render_obj.mesh.vertices[face[n]];
                ColorPolygon(vertices);
            }

            pictureBox1.Refresh();
            */

            //}

        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            

            if (e.Button == MouseButtons.Middle)
            {
                previousLocation = new vector((float)e.Location.X, (float)e.Location.Y);
                cursor_moving = true;
                cursor_x = e.X;
                cursor_y = e.Y;
                middle_mouse_click = true;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            cursor_moving = false;
            //cursor_x = -1;
            //cursor_y = -1;

            if (e.Button == MouseButtons.Middle) middle_mouse_click = false;
            
        }

        private void render_refresh_timer_Tick(object sender, EventArgs e)
        {
            //Current 1 fps
            update_view();
            
            


            if (cursor_x < 0 || cursor_y < 0 || cursor_x >= pictureBox1.Width || cursor_y >= pictureBox1.Height) return;
            //((Bitmap)pictureBox1.Image).SetPixel(cursor_x, cursor_y, pen.Color);

           
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
    public static class AllScripts
    {
        public static SceneInfo sceneInfo;
        public static Form1 form1;
        public static MeshModifierPanel meshModifierPanel;
        public static UserControl1 userControl1;
    }
}
