using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace DCS_AECIS
{
    class DataUpdater
    {
        private GameCamera gameCamera;  // set by constructor, or can I create this camera object in this constructor?
        private Timer timer;


        public int      UpdateInterval  { get; set; }           = 10;
        public bool     DcsConnected    { get; set; }           = false; // if connected to dcs
        public string   IpAddress       { get; set; }           = "127.0.0.1"; // if difference than localhost ip
        public int      Port            { get; set; }           = 3012; // tcp port used for connection
        public int      ConnCounter     { get; private set; }   = 0;  // how many connections have been made

        // camera data
        public LoVec3   CameraPosition
        {
            get
            {
                return gameCamera.P;
            }
        }

        public LoVec3 CameraOrientationX
        {
            get
            {
                return gameCamera.X;
            }
        }
        public LoVec3 CameraOrientationY
        {
            get
            {
                return gameCamera.Y;
            }
        }
        public LoVec3 CameraOrientationZ
        {
            get
            {
                return gameCamera.Z;
            }
        }

        
        public DataUpdater(GameCamera gameCamera)
        {
            this.gameCamera = gameCamera;
        }

        public void Connect() { timer = new Timer(ConnectAndUpdate, null, 0, UpdateInterval); }

        public void Disconnect() { timer.Dispose(); DcsConnected = false; }

        public void ConnectAndUpdate(object state)  // try connect to DCS via TCP socket
        {
            try
            {
                TcpClient client = new TcpClient(IpAddress, Port);
                NetworkStream nws = client.GetStream();

                StreamReader reader = new StreamReader(nws);
                StreamWriter writer = new StreamWriter(nws) { AutoFlush = true };

                var data = reader.ReadLine();
                ParseData(data);

                DcsConnected = true;
                ConnCounter += 1;
            }
            catch (Exception WhateverException)
            {
                DcsConnected = false;  // if failed, wait some time and try again?
            }
            

            //string updateJsonString = reader.ReadLine();

            //ParseData(updateJsonString);

            //var jsonData = PrepareCommandData(_gameCamera);

            //writer.Write(jsonData);

        }

        public void ParseData(string cameraDataJson)
        {
            var updateData = JsonConvert.DeserializeObject<Model.DcsCameraData>(cameraDataJson);
            gameCamera.X = updateData.X;
            gameCamera.Y = updateData.Y;
            gameCamera.Z = updateData.Z;
            gameCamera.P = updateData.P;
        }

        public string PrepareCommandData(GameCamera gameCamera)
        {
            // retrieve data from gameCamera to construct a new CameraCommandData
            // serialize this CameraCommandData and send string via tcp
            var commandData = new CameraCommandData
            {
                P = gameCamera.P,
                X = gameCamera.X,
                Y = gameCamera.Y,
                Z = gameCamera.Z,
                Camera_command = 0,
                // Camera_params
            };
            return JsonConvert.SerializeObject(commandData);
        }
    }
}
