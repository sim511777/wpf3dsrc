﻿//#define SHOW_AXES

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Media.Media3D;

namespace Materials
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // The camera.
        private PerspectiveCamera TheCamera = null;

        // The camera controller.
        private SphericalCameraController CameraController = null;

        // The model group.
        private Model3DGroup MainGroup = null;

        // The surface's mesh and model.
        private MeshGeometry3D SurfaceMesh = null;
        private Model3D SurfaceModel = null;

        // The main point light.
        private PointLight MainLight = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Define WPF objects.
            ModelVisual3D visual3d = new ModelVisual3D();
            MainGroup = new Model3DGroup();
            visual3d.Content = MainGroup;
            mainViewport.Children.Add(visual3d);

            // Define the camera, lights, and model.
            DefineCamera(mainViewport);
            DefineLights(MainGroup);
            DefineModel(MainGroup);
        }

        // Define the camera.
        private void DefineCamera(Viewport3D viewport)
        {
            TheCamera = new PerspectiveCamera();
            TheCamera.FieldOfView = 60;
            CameraController = new SphericalCameraController
                (TheCamera, viewport, this, mainGrid, mainGrid);
        }

        // Define the lights.
        private void DefineLights(Model3DGroup group)
        {
            group.Children.Add(new AmbientLight(Color.FromArgb(255, 128, 128, 128)));

            MainLight = new PointLight(Colors.Gray, new Point3D(0, 3, 2));
            group.Children.Add(MainLight);
        }

        // Define the model.
        private void DefineModel(Model3DGroup group)
        {
            // Make the surface mesh.
            SurfaceMesh = new MeshGeometry3D();
            AddSurface(SurfaceMesh, -3, -3, 3, 3, 50, 50, F);
            SurfaceModel = new GeometryModel3D(SurfaceMesh, null);
            group.Children.Add(SurfaceModel);

            // Make a "sun."
            MeshGeometry3D sun_mesh = MakeCubeMesh(
                MainLight.Position.X,
                MainLight.Position.Y,
                MainLight.Position.Z,
                0.25);
            MaterialGroup sun_group = new MaterialGroup();
            sun_group.Children.Add(new DiffuseMaterial(Brushes.Yellow));
            sun_group.Children.Add(new EmissiveMaterial(Brushes.Yellow));
            GeometryModel3D sun_model = new GeometryModel3D(sun_mesh, sun_group);
            group.Children.Add(sun_model);

#if SHOW_AXES
            // Make coordinate axis cubes.
            const double dist = 3;
            const double width = 0.5;
            MeshGeometry3D x_mesh = MakeCubeMesh(dist, 0, 0, width);
            DiffuseMaterial x_material = new DiffuseMaterial(Brushes.Red);
            GeometryModel3D x_model = new GeometryModel3D(x_mesh, x_material);
            group.Children.Add(x_model);

            MeshGeometry3D y_mesh = MakeCubeMesh(0, dist, 0, width);
            DiffuseMaterial y_material = new DiffuseMaterial(Brushes.Green);
            GeometryModel3D y_model = new GeometryModel3D(y_mesh, y_material);
            group.Children.Add(y_model);

            MeshGeometry3D z_mesh = MakeCubeMesh(0, 0, dist, width);
            DiffuseMaterial z_material = new DiffuseMaterial(Brushes.Blue);
            GeometryModel3D z_model = new GeometryModel3D(z_mesh, z_material);
            group.Children.Add(z_model);
#endif

            // Make materials. Order matters!
            ImageBrush smileyBrush = new ImageBrush();
            smileyBrush.ImageSource = new BitmapImage(new Uri("Smiley.png", UriKind.Relative));
            AddMaterial(new DiffuseMaterial(smileyBrush), "Image");

            AddMaterial(new DiffuseMaterial(Brushes.LightBlue), "Diffuse");

            LinearGradientBrush linear = new LinearGradientBrush();
            linear.GradientStops.Add(new GradientStop(Colors.Blue, 0.0));
            linear.GradientStops.Add(new GradientStop(Colors.White, 0.4));
            linear.GradientStops.Add(new GradientStop(Colors.Green, 0.5));
            linear.GradientStops.Add(new GradientStop(Colors.White, 0.6));
            linear.GradientStops.Add(new GradientStop(Colors.Red, 1.0));
            linear.StartPoint = new Point(1, 0);
            linear.EndPoint = new Point(0, 1);
            AddMaterial(new DiffuseMaterial(linear), "Linear");

            RadialGradientBrush radial = new RadialGradientBrush();
            radial.GradientStops.Add(new GradientStop(Colors.Blue, 0.0));
            radial.GradientStops.Add(new GradientStop(Colors.White, 0.25));
            radial.GradientStops.Add(new GradientStop(Colors.Green, 0.5));
            radial.GradientStops.Add(new GradientStop(Colors.White, 0.75));
            radial.GradientStops.Add(new GradientStop(Colors.Red, 1.0));
            AddMaterial(new DiffuseMaterial(radial), "Radial");

            AddMaterial(new SpecularMaterial(Brushes.LightBlue, 100), "Specular");

            AddMaterial(new EmissiveMaterial(Brushes.DarkBlue), "Emissive");
        }

        // Add a surface to the mesh.
        private void AddSurface(MeshGeometry3D mesh,
            double xmin, double zmin, double xmax, double zmax,
            int numx, int numz, Func<double, double, double> F)
        {
            double dx = (xmax - xmin) / numx;
            double dz = (zmax - zmin) / numz;

            // Make the points.
            double x = xmin;
            for (int i = 0; i <= numx; i++)
            {
                double z = zmin;
                for (int j = 0; j <= numz; j++)
                {
                    double y = F(x, z);
                    mesh.Positions.Add(new Point3D(x, y, z));
                    mesh.TextureCoordinates.Add(
                        new Point(x / (xmax - xmin), z / (zmax - zmin)));
                    z += dz;
                }
                x += dx;
            }

            // Make the triangles.
            for (int i = 0; i < numx; i++)
            {
                for (int j = 0; j < numz; j++)
                {
                    int i1 = i * (numz + 1) + j;
                    int i2 = i1 + 1;
                    int i3 = i2 + (numz + 1);
                    int i4 = i3 - 1;
                    mesh.TriangleIndices.Add(i1);
                    mesh.TriangleIndices.Add(i2);
                    mesh.TriangleIndices.Add(i3);

                    mesh.TriangleIndices.Add(i1);
                    mesh.TriangleIndices.Add(i3);
                    mesh.TriangleIndices.Add(i4);
                }
            }
        }

        // Return a function F(x, z);
        private double F(double x, double z)
        {
            return 2 - (x * x + z * z) / 5;
        }

        // Add a material and its CheckBox.
        private void AddMaterial(Material material, string caption)
        {
            CheckBox rad = new CheckBox();
            rad.Tag = material;
            rad.Content = caption;
            rad.Click += UseMaterials;

            materialWrapPanel.Children.Add(rad);
        }

        // Use the selected materials.
        private void UseMaterials(object sender, RoutedEventArgs e)
        {
            // Remove the existing model.
            MainGroup.Children.Remove(SurfaceModel);

            // Make a new MaterialGroup holding the selected materials.
            MaterialGroup group = new MaterialGroup();
            foreach (CheckBox chk in materialWrapPanel.Children)
            {
                if (chk.IsChecked.Value)
                    group.Children.Add(chk.Tag as Material);
            }

            // Add a new model using the new material.
            SurfaceModel = new GeometryModel3D(SurfaceMesh, group);
            MainGroup.Children.Add(SurfaceModel);
        }

        // Make a mesh containing a cube centered at this point.
        private MeshGeometry3D MakeCubeMesh(double x, double y, double z, double width)
        {
            // Create the geometry.
            MeshGeometry3D mesh = new MeshGeometry3D();

            // Define the positions.
            width /= 2;
            Point3D[] points =
            {
                new Point3D(x - width, y - width, z - width),
                new Point3D(x + width, y - width, z - width),
                new Point3D(x + width, y - width, z + width),
                new Point3D(x - width, y - width, z + width),
                new Point3D(x - width, y - width, z + width),
                new Point3D(x + width, y - width, z + width),
                new Point3D(x + width, y + width, z + width),
                new Point3D(x - width, y + width, z + width),
                new Point3D(x + width, y - width, z + width),
                new Point3D(x + width, y - width, z - width),
                new Point3D(x + width, y + width, z - width),
                new Point3D(x + width, y + width, z + width),
                new Point3D(x + width, y + width, z + width),
                new Point3D(x + width, y + width, z - width),
                new Point3D(x - width, y + width, z - width),
                new Point3D(x - width, y + width, z + width),
                new Point3D(x - width, y - width, z + width),
                new Point3D(x - width, y + width, z + width),
                new Point3D(x - width, y + width, z - width),
                new Point3D(x - width, y - width, z - width),
                new Point3D(x - width, y - width, z - width),
                new Point3D(x - width, y + width, z - width),
                new Point3D(x + width, y + width, z - width),
                new Point3D(x + width, y - width, z - width),
            };
            foreach (Point3D point in points) mesh.Positions.Add(point);

            // Define the triangles.
            Tuple<int, int, int>[] triangles =
            {
                 new Tuple<int, int, int>(0, 1, 2),
                 new Tuple<int, int, int>(2, 3, 0),
                 new Tuple<int, int, int>(4, 5, 6),
                 new Tuple<int, int, int>(6, 7, 4),
                 new Tuple<int, int, int>(8, 9, 10),
                 new Tuple<int, int, int>(10, 11, 8),
                 new Tuple<int, int, int>(12, 13, 14),
                 new Tuple<int, int, int>(14, 15, 12),
                 new Tuple<int, int, int>(16, 17, 18),
                 new Tuple<int, int, int>(18, 19, 16),
                 new Tuple<int, int, int>(20, 21, 22),
                 new Tuple<int, int, int>(22, 23, 20),
            };
            foreach (Tuple<int, int, int> tuple in triangles)
            {
                mesh.TriangleIndices.Add(tuple.Item1);
                mesh.TriangleIndices.Add(tuple.Item2);
                mesh.TriangleIndices.Add(tuple.Item3);
            }

            return mesh;
        }
    }
}
