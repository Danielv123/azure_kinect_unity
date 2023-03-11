using Assets.Scripts.Classes;
using Microsoft.Azure.Kinect.Sensor;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class AzureKinect
{
    public bool HasChanged { get; set; } = false;
    public Color[] positions = new Color[0];
    public Color[] colors = new Color[0];

    #region Camera control
    private bool _applicationIsRunning = true;
    private Device _device;
    private SynchronizationContext _uiContext;
    private Transformation _transformation;
    private int _colorWidth;
    private int _colorHeight;
    private int _depthWidth;
    private int _depthHeight;
    private int _depthPoints;
    private bool firstCycle = true;

    internal void StartCamera(int cameraNo = 0)
    {
        if (Device.GetInstalledCount() == 0)
        {
            throw new Exception("No Azure Kinect devices detected.");
        }

        _device = Device.Open(cameraNo);

        var configuration = new DeviceConfiguration
        {
            ColorFormat = ImageFormat.ColorBGRA32, // Required for depth color merging
            ColorResolution = ColorResolution.R1536p,
            CameraFPS = FPS.FPS15,
            DepthMode = DepthMode.WFOV_Unbinned, // Camera fails when unbinned unless lowering FPS
            SynchronizedImagesOnly = true, // External sync
            DisableStreamingIndicator = true
        };

        _device.StartCameras(configuration);

        _device.StartImu();

        var calibration = _device.GetCalibration(configuration.DepthMode, configuration.ColorResolution);
        _transformation = calibration.CreateTransformation();
        _colorWidth = calibration.ColorCameraCalibration.ResolutionWidth;
        _colorHeight = calibration.ColorCameraCalibration.ResolutionHeight;
        _depthWidth = calibration.DepthCameraCalibration.ResolutionWidth;
        _depthHeight = calibration.DepthCameraCalibration.ResolutionHeight;
        _depthPoints = _depthWidth * _depthHeight;

        _uiContext = SynchronizationContext.Current;

        _applicationIsRunning = true;

        //Task.Run(() => { ImuCapture(); });
        Task.Run(() => { CameraCapture(); });
    }
    internal void StopCamera()
    {
        _applicationIsRunning = false;

        // Allow camera tasks time to stop
        Task.WaitAny(Task.Delay(1000));

        _device.StopCameras();
        //_device.StopImu();
        _device.Dispose();
    }
    private void CameraCapture()
    {
        while (_applicationIsRunning)
        {
            // Need to dispose of afterwards
            using (var capture = _device.GetCapture())
            {
                try
                {
                    BuildDepthImageSource(capture);
                }
                catch (Exception e)
                {
                    Debug.Log("Error: " + e.Message);
                    _applicationIsRunning = false;
                }
            }
        }
    }
    Short3[] pointCloud;
    BGRA[] colorArray;
    Color[] new_positions;
    Color[] new_colors;
    private void BuildDepthImageSource(Capture capture)
    {
        var pImage = _transformation.DepthImageToPointCloud(capture.Depth); // Create point cloud from depth image
        var modifiedColor = _transformation.ColorImageToDepthCamera(capture);
        _uiContext.Send(x =>
        {
            // Get the transformed pixels
            pointCloud = pImage.GetPixels<Short3>().ToArray(); // Array of xyz triplets
            colorArray = modifiedColor.GetPixels<BGRA>().ToArray(); // Array of BGRA pixels corresponding to pointcloud

            // What we'll output.
            if (firstCycle)
            {
                new_positions = new Color[_depthPoints];
                new_colors = new Color[_depthPoints];
                firstCycle = false;
            }

            for (int i = 0; i < _depthPoints; i++)
            {
                //new_positions[i] = new Color(pointCloud[i].X * 0.01f, -pointCloud[i].Y * 0.01f, pointCloud[i].Z * 0.01f, 0.1f); // Alpha value is particle size
                //new_colors[i] = new Color32(colorArray[i].R, colorArray[i].G, colorArray[i].B, 255); //colorArray[i].A);
                // Updating existing color structs is at least 3x faster
                new_positions[i].r = pointCloud[i].X * 0.01f;
                new_positions[i].g = pointCloud[i].Y * -0.01f;
                new_positions[i].b = pointCloud[i].Z * 0.01f;
                new_positions[i].a = 0.1f; // Particle size
                new_colors[i].r = colorArray[i].R / 255f;
                new_colors[i].g = colorArray[i].G / 255f;
                new_colors[i].b = colorArray[i].B / 255f;
                new_colors[i].a = 1;
            }

            HasChanged = true;
            positions = new_positions;
            colors = new_colors;

            // Free up memory
            pImage.Dispose();
            modifiedColor.Dispose();
        }, null);
        pImage.Dispose();
        modifiedColor.Dispose();
    }

    private void ImuCapture()
    {
        while (_applicationIsRunning)
        {
            try
            {
                var imu = _device.GetImuSample();

                //AddOrUpdateDeviceData("Accelerometer timestamp: ", imu.AccelerometerTimestamp.ToString(@"hh\:mm\:ss"));
                //AddOrUpdateDeviceData("Accelerometer X", Math.Round(imu.AccelerometerSample.X, 2).ToString());
                //AddOrUpdateDeviceData("Accelerometer Y", Math.Round(imu.AccelerometerSample.Y, 2).ToString());
                //AddOrUpdateDeviceData("Accelerometer Z", Math.Round(imu.AccelerometerSample.Z, 2).ToString());
                //AddOrUpdateDeviceData("Gyro timestamp", imu.GyroTimestamp.ToString(@"hh\:mm\:ss"));
                //AddOrUpdateDeviceData("Gyro X", Math.Round(imu.GyroSample.X, 2).ToString());
                //AddOrUpdateDeviceData("Gyro Y", Math.Round(imu.GyroSample.Y, 2).ToString());
                //AddOrUpdateDeviceData("Gyro Z", Math.Round(imu.GyroSample.Z, 2).ToString());
                //AddOrUpdateDeviceData("Temperature", Math.Round(imu.Temperature, 2).ToString());
            }
            catch (Exception ex)
            {
                _applicationIsRunning = false;
                Debug.Log("IMU failed");
                Debug.Log(ex.Message);
            }
        }
    }
    #endregion Camera control
}
