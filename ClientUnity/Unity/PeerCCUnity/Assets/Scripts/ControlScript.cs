//#define staticPanel

using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;
using System.Threading;


#if !UNITY_EDITOR
using Windows.UI.Core;
using Windows.Foundation;
using Windows.Media.Core;
using System.Linq;
using System.Threading.Tasks;
using PeerConnectionClient.Signalling;
using Windows.ApplicationModel.Core;
#endif

public class ControlScript : MonoBehaviour
{
    public static ControlScript Instance { get; private set; }

    public uint LocalTextureWidth = 160;
    public uint LocalTextureHeight = 120;
    public uint RemoteTextureWidth = 640;
    public uint RemoteTextureHeight = 480;

    public GameObject LocalVideoImage;
    
    public InputField ServerAddressInputField;
    public Button ConnectButton;
    public Button CallButton;
    public RectTransform PeerContent;
    public GameObject TextItemPreftab;

    public Canvas Canvas;
    public Material Material;
    public Texture Texture;

    List<GameObject> gameObjectList;
    List<string> namePanelList;
    RawImage rawImage;

    RectTransform rect;

    int col = 0;
    int row = 0;

    float xPos = 0f;
    float yPos = 0f;
    float zPos = 0f;

    const float xRemoteDimensions = 640f;
    const float yRemoteDimensions = 480f;

    float xDimensions = 640f;
    float yDimensions = 480f;


    private enum Status
    {
        NotConnected,
        Connecting,
        Disconnecting,
        Connected,
        Calling,
        EndingCall,
        InCall
    }

    private enum CommandType
    {
        Empty,
        SetNotConnected,
        SetConnected,
        SetInCall,
        AttachLocalPanel,
        DetachLocalPanel,
        AddRemotePanel,
        RemoveRemotePanel,
        RemoveAllRemotePanel
    }

    private struct Command
    {
        public CommandType type;
#if !UNITY_EDITOR
        public Conductor.Peer remotePeer;
#endif
    }

    private Status status = Status.NotConnected;
    private List<Command> commandQueue = new List<Command>();
    //private int selectedPeerIndex = -1;

    public ControlScript()
    {
    }

    private void Awake()
    {
    }

    private void Start()
    {
        Instance = this;
        gameObjectList = new List<GameObject>();
        namePanelList = new List<string>();
#if !UNITY_EDITOR
        Conductor.Instance.Initialized += Conductor_Initialized;
        Conductor.Instance.Initialize(CoreApplication.MainView.CoreWindow.Dispatcher);
        Conductor.Instance.EnableLogging(Conductor.LogLevel.Verbose);
#endif
        ServerAddressInputField.text = "janus.runamedia.com";
    }

    private void OnEnable()
    {       
        {
            Plugin.CreateMediaPlayback("LocalVideo");
            IntPtr nativeTex = IntPtr.Zero;
            Plugin.GetPrimaryTexture("LocalVideo", LocalTextureWidth, LocalTextureHeight, out nativeTex);
            var primaryPlaybackTexture = Texture2D.CreateExternalTexture((int)LocalTextureWidth, (int)LocalTextureHeight, TextureFormat.BGRA32, false, false, nativeTex);
            rawImage = LocalVideoImage.GetComponent<RawImage>();
            rawImage.texture = primaryPlaybackTexture;
           
        }
    }

    private void OnDisable()
    {
        rawImage.texture = null;
        Plugin.ReleaseMediaPlayback("LocalVideo");
        rawImage.texture = Texture;
    }

#if false
    private void LateUpdate()
    {
        lock (this)
        {
            if (lateCounter < remotePanelCounter)
            {
                CreateRemotePanel();
                CreateMedia();
                lateCounter++;
                UpdateEvent.Set();
            }
            else if (lateCounter > remotePanelCounter)
            {
                for (int i = 0; i < gameObjectList.Count; i++)
                {
                    DestroyObject(gameObjectList[i]);
                }
                lateCounter--;
                gameObjectList.Clear();
                namePanelList.Clear();
                row = 0;
                col = 0;
            }
            if (Input.GetKeyDown("n"))
            {
                remotePanelCounter++;
                CreateRemotePanel();
            }
        }
    }
#endif
    private void LateUpdate()
    {
#if !UNITY_EDITOR
        lock (this)
        {
            switch (status)
            {
                case Status.NotConnected:
                    if (!ServerAddressInputField.enabled)
                        ServerAddressInputField.enabled = true;
                    if (!ConnectButton.enabled)
                        ConnectButton.enabled = true;
                    if (CallButton.enabled)
                        CallButton.enabled = false;
                    break;
                case Status.Connecting:
                    if (ServerAddressInputField.enabled)
                        ServerAddressInputField.enabled = false;
                    if (ConnectButton.enabled)
                        ConnectButton.enabled = false;
                    if (CallButton.enabled)
                        CallButton.enabled = false;
                    break;
                case Status.Disconnecting:
                    if (ServerAddressInputField.enabled)
                        ServerAddressInputField.enabled = false;
                    if (ConnectButton.enabled)
                        ConnectButton.enabled = false;
                    if (CallButton.enabled)
                        CallButton.enabled = false;
                    break;
                case Status.Connected:
                    if (ServerAddressInputField.enabled)
                        ServerAddressInputField.enabled = false;
                    if (!ConnectButton.enabled)
                        ConnectButton.enabled = true;
                    if (!CallButton.enabled)
                        CallButton.enabled = true;
                    break;
                case Status.Calling:
                    if (ServerAddressInputField.enabled)
                        ServerAddressInputField.enabled = false;
                    if (ConnectButton.enabled)
                        ConnectButton.enabled = false;
                    if (CallButton.enabled)
                        CallButton.enabled = false;
                    break;
                case Status.EndingCall:
                    if (ServerAddressInputField.enabled)
                        ServerAddressInputField.enabled = false;
                    if (ConnectButton.enabled)
                        ConnectButton.enabled = false;
                    if (CallButton.enabled)
                        CallButton.enabled = false;
                    break;
                case Status.InCall:
                    if (ServerAddressInputField.enabled)
                        ServerAddressInputField.enabled = false;
                    if (ConnectButton.enabled)
                        ConnectButton.enabled = false;
                    if (!CallButton.enabled)
                        CallButton.enabled = true;
                    break;
                default:
                    break;
            }

            while (commandQueue.Count != 0)
            {
                Command command = commandQueue.First();
                commandQueue.RemoveAt(0);
                switch (status)
                {
                    case Status.NotConnected:
                        if (command.type == CommandType.SetNotConnected)
                        {
                            ConnectButton.GetComponentInChildren<Text>().text = "Connect";
                            CallButton.GetComponentInChildren<Text>().text = "Call";
                        }
                        break;
                    case Status.Connected:
                        if (command.type == CommandType.SetConnected)
                        {
                            ConnectButton.GetComponentInChildren<Text>().text = "Disconnect";
                            CallButton.GetComponentInChildren<Text>().text = "Call";
                        }
                        break;
                    case Status.InCall:
                        if (command.type == CommandType.SetInCall)
                        {
                            ConnectButton.GetComponentInChildren<Text>().text = "Disconnect";
                            CallButton.GetComponentInChildren<Text>().text = "Hang Up";
                        }
                        break;
                    default:
                        break;
                }
                if (command.type == CommandType.AttachLocalPanel)
                {
                }
                else if (command.type == CommandType.DetachLocalPanel)
                {
                }
                else if (command.type == CommandType.AddRemotePanel)
                {
                    //GameObject textItem = (GameObject)Instantiate(TextItemPreftab);
                    //textItem.transform.SetParent(PeerContent);
                    //textItem.GetComponent<Text>().text = command.remotePeer.Name;
                    //EventTrigger trigger = textItem.GetComponentInChildren<EventTrigger>();
                    //EventTrigger.Entry entry = new EventTrigger.Entry();
                    //entry.eventID = EventTriggerType.PointerDown;
                    //entry.callback.AddListener((data) => { OnRemotePeerItemClick((PointerEventData)data); });
                    //trigger.triggers.Add(entry);
                    //if (selectedPeerIndex == -1)
                    //{
                    //    textItem.GetComponent<Text>().fontStyle = FontStyle.Bold;
                    //    selectedPeerIndex = PeerContent.transform.childCount - 1;
                    //}
                    CreateRemotePanel("Panel-" + command.remotePeer.Id.ToString());
                    CreateMedia("Panel-" + command.remotePeer.Id.ToString());
#if !UNITY_EDITOR
                    var task = RunOnUiThread(() =>
                    {
                        lock (this)
                        {
                            if (status == Status.InCall || status == Status.Connected)
                            {
                                IMediaSource source;
                                if (Conductor.Instance.VideoCodec.Name == "H264")
                                    source = Conductor.Instance.CreateRemoteMediaStreamSource(command.remotePeer.Id, "H264");
                                else
                                    source = Conductor.Instance.CreateRemoteMediaStreamSource(command.remotePeer.Id, "I420");
                                Plugin.LoadMediaStreamSource("Panel-" + command.remotePeer.Id.ToString(), (MediaStreamSource)source);
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Conductor.OnAddRemoteStream() - wrong status - " + status);
                            }
                        }
                    });
#endif 
                }
                else if (command.type == CommandType.RemoveRemotePanel)
                {
                    //for (int i = 0; i < PeerContent.transform.childCount; i++)
                    //{
                    //    if (PeerContent.GetChild(i).GetComponent<Text>().text == command.remotePeer.Name)
                    //    {
                    //        PeerContent.GetChild(i).SetParent(null);
                    //        if (selectedPeerIndex == i)
                    //        {
                    //            if (PeerContent.transform.childCount > 0)
                    //            {
                    //                PeerContent.GetChild(0).GetComponent<Text>().fontStyle = FontStyle.Bold;
                    //                selectedPeerIndex = 0;
                    //            }
                    //            else
                    //            {
                    //                selectedPeerIndex = -1;
                    //            }
                    //        }
                    //        break;
                    //    }
                    //}
                    int indexForRemove = 0;
                    for (int i = 0; i < gameObjectList.Count; i++)
                    {
                        if (namePanelList[i] == "Panel-" + command.remotePeer.Id.ToString())
                        {
                            DestroyObject(gameObjectList[i]);
                            indexForRemove = i;
                        }
                    }
                    ReleaseMedia("Panel-" + command.remotePeer.Id.ToString());
#if !UNITY_EDITOR
                    var task = RunOnUiThread(() =>
                    {
                        lock (this)
                        {
                            if (status == Status.InCall || status == Status.Connected)
                            {
                                Plugin.UnloadMediaStreamSource("Panel-" + command.remotePeer.Id.ToString());
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Conductor.OnRemoveRemoteStream() - wrong status - " + status);
                            }
                        }
                    });
                    gameObjectList.RemoveAt(indexForRemove);
                    namePanelList.RemoveAt(indexForRemove);
                    RemoveRemotePanel();
#endif
                }
                else if (command.type == CommandType.RemoveAllRemotePanel)
                {
                    foreach (var item in namePanelList)
                    {
                        Plugin.UnloadMediaStreamSource(item);
                        ReleaseMedia(item);
                    }
                    foreach (var item in gameObjectList)
                    {
                        DestroyObject(item);
                    }
                    gameObjectList.Clear();
                    namePanelList.Clear();
                    row = 0;
                    col = 0;
                    status = Status.Connected;
                    commandQueue.Add(new Command { type = CommandType.SetConnected});
                }
            }
        }
#endif
    }

    private void Conductor_Initialized(bool succeeded)
    {
        if (succeeded)
        {
            Initialize();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Conductor initialization failed");
        }
    }

    public void OnConnectClick()
    {
#if !UNITY_EDITOR
        lock (this)
        {
            if (status == Status.NotConnected)
            {
                new Task(() =>
                {
                    Conductor.Instance.StartLogin(ServerAddressInputField.text, "8088");
                }).Start();
                status = Status.Connecting;
            }
            else if (status == Status.Connected)
            {
                new Task(() =>
                {
                    var task = Conductor.Instance.DisconnectFromServer();
                }).Start();

                status = Status.Disconnecting;
                //selectedPeerIndex = -1;
                PeerContent.DetachChildren();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("OnConnectClick() - wrong status - " + status);
            }
        }
#endif
    }

    public void OnCallClick()
    {
#if !UNITY_EDITOR
        lock (this)
        {
            if (status == Status.Connected)
            {
                //    if (selectedPeerIndex == -1)
                //        return;
                new Task(() =>
                {
                    //Conductor.Peer conductorPeer = Conductor.Instance.GetPeers()[selectedPeerIndex];
                    //if (conductorPeer != null)
                    //{
                    //    Conductor.Instance.ConnectToPeer(conductorPeer);
                    //}
                    Conductor.Instance.ConnectToPeer(null);
                }).Start();
                status = Status.Calling;
            }
            else if (status == Status.InCall)
            {
                new Task(() =>
                {
                    var task = Conductor.Instance.DisconnectFromPeer();
                }).Start();
                status = Status.EndingCall;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("OnCallClick() - wrong status - " + status);
            }
        }
#endif
    }

    public void OnRemotePeerItemClick(PointerEventData data)
    {
#if !UNITY_EDITOR
        //for (int i = 0; i < PeerContent.transform.childCount; i++)
        //{
        //    if (PeerContent.GetChild(i) == data.selectedObject.transform)
        //    {
        //        data.selectedObject.GetComponent<Text>().fontStyle = FontStyle.Bold;
        //        //selectedPeerIndex = i;
        //    }
        //    else
        //    {
        //        PeerContent.GetChild(i).GetComponent<Text>().fontStyle = FontStyle.Normal;
        //    }
        //}
#endif
    }

#if !UNITY_EDITOR
    public async Task OnAppSuspending()
    {
        Conductor.Instance.CancelConnectingToPeer();

        await Conductor.Instance.DisconnectFromPeer();
        await Conductor.Instance.DisconnectFromServer();

        Conductor.Instance.OnAppSuspending();
    }

    private IAsyncAction RunOnUiThread(Action fn)
    {
        return CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(fn));
    }
#endif

    public void Initialize()
    {
#if !UNITY_EDITOR
        // A Peer is connected to the server event handler
        Conductor.Instance.Signaller.OnPeerConnected += (peerId, peerName) =>
        {
            //var task = RunOnUiThread(() =>
            //{
            //    lock (this)
            //    {
            //        Conductor.Peer peer = new Conductor.Peer { Id = peerId, Name = peerName };
            //        Conductor.Instance.AddPeer(peer);
            //        commandQueue.Add(new Command { type = CommandType.AddRemotePeer, remotePeer = peer });
            //    }
            //});
        };

        // A Peer is disconnected from the server event handler
        Conductor.Instance.Signaller.OnPeerDisconnected += peerId =>
        {
            //var task = RunOnUiThread(() =>
            //{
            //    lock (this)
            //    {
            //        var peerToRemove = Conductor.Instance.GetPeers().FirstOrDefault(p => p.Id == peerId);
            //        if (peerToRemove != null)
            //        {
            //            Conductor.Peer peer = new Conductor.Peer { Id = peerToRemove.Id, Name = peerToRemove.Name };
            //            Conductor.Instance.RemovePeer(peer);
            //            commandQueue.Add(new Command { type = CommandType.RemoveRemotePeer, remotePeer = peer });
            //        }
            //    }
            //});
        };

        // The user is Signed in to the server event handler
        Conductor.Instance.Signaller.OnSignedIn += () =>
        {
            var task = RunOnUiThread(() =>
            {
                lock (this)
                {
                    if (status == Status.Connecting)
                    {
                        status = Status.Connected;
                        commandQueue.Add(new Command { type = CommandType.SetConnected });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Signaller.OnSignedIn() - wrong status - " + status);
                    }
                }
            });
        };

        // Failed to connect to the server event handler
        Conductor.Instance.Signaller.OnServerConnectionFailure += () =>
        {
            var task = RunOnUiThread(() =>
            {
                lock (this)
                {
                    if (status == Status.Connecting)
                    {
                        status = Status.NotConnected;
                        commandQueue.Add(new Command { type = CommandType.SetNotConnected });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Signaller.OnServerConnectionFailure() - wrong status - " + status);
                    }
                }
            });
        };

        // The current user is disconnected from the server event handler
        Conductor.Instance.Signaller.OnDisconnected += () =>
        {
            var task = RunOnUiThread(() =>
            {
                lock (this)
                {
                    if (status == Status.Disconnecting)
                    {
                        status = Status.NotConnected;
                        commandQueue.Add(new Command { type = CommandType.SetNotConnected });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Signaller.OnDisconnected() - wrong status - " + status);
                    }
                }
            });
        };

        Conductor.Instance.OnAddRemoteStream += Conductor_OnAddRemoteStream;
        Conductor.Instance.OnRemoveRemoteStream += Conductor_OnRemoveRemoteStream;
        Conductor.Instance.OnAddLocalStream += Conductor_OnAddLocalStream;

        // Connected to a peer event handler
        Conductor.Instance.OnPeerConnectionCreated += () =>
        {
            var task = RunOnUiThread(() =>
            {
                lock (this)
                {
                    if (status == Status.Calling || status == Status.Connected)
                    {
                        status = Status.InCall;
                        commandQueue.Add(new Command { type = CommandType.SetInCall });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Conductor.OnPeerConnectionCreated() - wrong status - " + status);
                    }
                }
            });
        };

        // Connection between the current user and a peer is closed event handler
        Conductor.Instance.OnPeerConnectionClosed += () =>
        {
            var task = RunOnUiThread(() =>
            {
                lock (this)
                {
                    if (status == Status.EndingCall || status == Status.InCall)
                    {
                        Plugin.UnloadMediaStreamSource("LocalVideo");
                        commandQueue.Add(new Command { type = CommandType.RemoveAllRemotePanel });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Conductor.OnPeerConnectionClosed() - wrong status - " + status);
                    }
                }
            });
        };

        // Ready to connect to the server event handler
        Conductor.Instance.OnReadyToConnect += () => { var task = RunOnUiThread(() => { }); };

        List<Conductor.IceServer> iceServers = new List<Conductor.IceServer>();
        iceServers.Add(new Conductor.IceServer { Host = "stun.l.google.com:19302", Type = Conductor.IceServer.ServerType.STUN });
        iceServers.Add(new Conductor.IceServer { Host = "stun1.l.google.com:19302", Type = Conductor.IceServer.ServerType.STUN });
        iceServers.Add(new Conductor.IceServer { Host = "stun2.l.google.com:19302", Type = Conductor.IceServer.ServerType.STUN });
        iceServers.Add(new Conductor.IceServer { Host = "stun3.l.google.com:19302", Type = Conductor.IceServer.ServerType.STUN });
        iceServers.Add(new Conductor.IceServer { Host = "stun4.l.google.com:19302", Type = Conductor.IceServer.ServerType.STUN });
        Conductor.IceServer turnServer = new Conductor.IceServer { Host = "turnserver3dstreaming.centralus.cloudapp.azure.com:5349", Type = Conductor.IceServer.ServerType.TURN };
        turnServer.Credential = "3Dtoolkit072017";
        turnServer.Username = "user";
        iceServers.Add(turnServer);
        Conductor.Instance.ConfigureIceServers(iceServers);

        var audioCodecList = Conductor.Instance.GetAudioCodecs();
        Conductor.Instance.AudioCodec = audioCodecList.FirstOrDefault(c => c.Name == "opus");
        System.Diagnostics.Debug.WriteLine("Selected audio codec - " + Conductor.Instance.AudioCodec.Name);

        // Order the video codecs so that the stable VP8 is in front.
        var videoCodecList = Conductor.Instance.GetVideoCodecs();
        Conductor.Instance.VideoCodec = videoCodecList.FirstOrDefault(c => c.Name == "VP8");
        System.Diagnostics.Debug.WriteLine("Selected video codec - " + Conductor.Instance.VideoCodec.Name);

        uint preferredWidth = 640;
        uint preferredHeght = 480;
        uint preferredFrameRate = 15;
        uint minSizeDiff = uint.MaxValue;
        Conductor.CaptureCapability selectedCapability = null;
        var videoDeviceList = Conductor.Instance.GetVideoCaptureDevices();
        foreach (Conductor.MediaDevice device in videoDeviceList)
        {
            Conductor.Instance.GetVideoCaptureCapabilities(device.Id).AsTask().ContinueWith(capabilities =>
            {
                foreach (Conductor.CaptureCapability capability in capabilities.Result)
                {
                    uint sizeDiff = (uint)Math.Abs(preferredWidth - capability.Width) + (uint)Math.Abs(preferredHeght - capability.Height);
                    if (sizeDiff < minSizeDiff)
                    {
                        selectedCapability = capability;
                        minSizeDiff = sizeDiff;
                    }
                    System.Diagnostics.Debug.WriteLine("Video device capability - " + device.Name + " - " + capability.Width + "x" + capability.Height + "@" + capability.FrameRate);
                }
            }).Wait();
        }

        if (selectedCapability != null)
        {
            selectedCapability.FrameRate = preferredFrameRate;
            //selectedCapability.MrcEnabled = true;
            Conductor.Instance.VideoCaptureProfile = selectedCapability;
            Conductor.Instance.UpdatePreferredFrameFormat();
            System.Diagnostics.Debug.WriteLine("Selected video device capability - " + selectedCapability.Width + "x" + selectedCapability.Height + "@" + selectedCapability.FrameRate);
        }
#endif
    }
#if !UNITY_EDITOR
    private void Conductor_OnAddRemoteStream(Conductor.Peer peer)
    {
        var task = RunOnUiThread(() =>
        {
            lock (this)
            {
                commandQueue.Add(new Command { type = CommandType.AddRemotePanel, remotePeer = peer });
            }
        });
#if false
        lock (lockObj)
        {
            remotePanelCounter++;
            UpdateEvent.WaitOne();


            var task = RunOnUiThread(() =>
            {
                lock (this)
                {
                    for (int i = 0; i < namePanelList.Count; i++)
                    {
                        if (status == Status.InCall || status == Status.Connected)
                        {
                            IMediaSource source;
                            if (Conductor.Instance.VideoCodec.Name == "H264")
                                source = Conductor.Instance.CreateRemoteMediaStreamSource("H264");
                            else
                                source = Conductor.Instance.CreateRemoteMediaStreamSource("I420");
                            Plugin.LoadMediaStreamSource(namePanelList[namePanelList.Count - 1], (MediaStreamSource)source);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Conductor.OnAddRemoteStream() - wrong status - " + status);
                        }
                        //mediaSourceCounter++;
                    }
                }
            });
        }
#endif

    }

    private void CreateRemotePanel(string panelName)
    {
        if (gameObjectList.Count() + 1 == 1)
        {
            col++;
            row++;
        }
        if (gameObjectList.Count() + 1 == 2)
        {
            col++;
        }
        else if (gameObjectList.Count() + 1 <= (row * col))
        {

        }
        else if (row == col)
        {
            col++;
        }
        else if (row != col)
        {
            row++;
        }      
        AddPanelToList(panelName);
        ChangePositionAndDimensions();
    }

    private void RemoveRemotePanel()
    {
        if (gameObjectList.Count > 0)
        {
            if (row == col)
            {
                if (gameObjectList.Count + row == row * col)
                {
                    row--;
                }
            }
            else if (row != col)
            {
                if (gameObjectList.Count + row == row * col)
                {
                    col--;
                }
            }
            ChangePositionAndDimensions();
        }
        else
        {
            row = 0;
            col = 0;
        }
    }

    private void AddPanelToList(string panelName)
    {
        gameObjectList.Add(new GameObject(panelName));
        namePanelList.Add(gameObjectList[gameObjectList.Count - 1].name);
        gameObjectList[gameObjectList.Count - 1].transform.SetParent(Canvas.transform, false);
        rawImage = gameObjectList[gameObjectList.Count - 1].AddComponent<RawImage>();
        rawImage.color = Color.black;
        rawImage.material = Material;
        rawImage.texture = Texture;                                     
    }
    private void ChangePositionAndDimensions()
    {
        xPos = 0;
        yPos = 0;
        //xDimensions = xRemoteDimensions / col;
        //yDimensions = yRemoteDimensions / row;

        for (int i = 0; i < gameObjectList.Count; i++)
        {
            rect = gameObjectList[i].GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(xDimensions, yDimensions);

            if (i == 0)
            {
                xPos = xRemoteDimensions / (col * 2) - xRemoteDimensions / 2;
            }
            else if (i % col == 0)
            {
                xPos = xRemoteDimensions / (col * 2) - xRemoteDimensions / 2;
            }
            else
            {
                xPos += xRemoteDimensions / col;
            }
            if (row >= 2)
            {
                if (i == 0)
                {
                    yPos = yRemoteDimensions / 2 - yRemoteDimensions / (row * 2);
                }
                else if (i % col == 0)
                {
                    yPos -= yRemoteDimensions / row;
                }
            }
            rect.anchoredPosition = new Vector2(xPos, 400);
        }
    }
#endif
    private void CreateMedia(String panelName)
    {
        Plugin.CreateMediaPlayback(panelName);
        IntPtr nativeTex = IntPtr.Zero;
        Plugin.GetPrimaryTexture(panelName, RemoteTextureWidth, RemoteTextureHeight, out nativeTex);
        var primaryPlaybackTexture = Texture2D.CreateExternalTexture((int)RemoteTextureWidth, (int)RemoteTextureHeight, TextureFormat.BGRA32, false, false, nativeTex);
        rawImage.texture = primaryPlaybackTexture;
    }

    private void ReleaseMedia(String panelName)
    {
        Plugin.ReleaseMediaPlayback(panelName);
}
#if !UNITY_EDITOR
    private void Conductor_OnRemoveRemoteStream(Conductor.Peer peer)
    {
        var task = RunOnUiThread(() =>
        {
            lock (this)
            {
                commandQueue.Add(new Command { type = CommandType.RemoveRemotePanel, remotePeer = peer });
            }
        });
    }
#endif
#if !UNITY_EDITOR
    private void Conductor_OnAddLocalStream()
    {

        var task = RunOnUiThread(() =>
        {
            lock (this)
            {
                if (status == Status.InCall || status == Status.Connected)
                {
                    var source = Conductor.Instance.CreateLocalMediaStreamSource("I420");
                    Plugin.LoadMediaStreamSource("LocalVideo", (MediaStreamSource)source);

                    Conductor.Instance.EnableLocalVideoStream();
                    Conductor.Instance.UnmuteMicrophone();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Conductor.OnAddLocalStream() - wrong status - " + status);
                }
            }
        });

    }
#endif
private static class Plugin
    {
        [DllImport("MediaEngineUWP", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateMediaPlayback")]
        internal static extern void CreateMediaPlayback([MarshalAs(UnmanagedType.LPWStr)]string playerId);

        [DllImport("MediaEngineUWP", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ReleaseMediaPlayback")]
        internal static extern void ReleaseMediaPlayback([MarshalAs(UnmanagedType.LPWStr)]string playerId);

        [DllImport("MediaEngineUWP", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetPrimaryTexture")]
        internal static extern void GetPrimaryTexture([MarshalAs(UnmanagedType.LPWStr)]string playerId, UInt32 width, UInt32 height, out System.IntPtr playbackTexture);

#if !UNITY_EDITOR
        [DllImport("MediaEngineUWP", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "LoadMediaStreamSource")]
        internal static extern void LoadMediaStreamSource([MarshalAs(UnmanagedType.LPWStr)]string playerId, MediaStreamSource IMediaSourceHandler);

        [DllImport("MediaEngineUWP", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "UnloadMediaStreamSource")]
        internal static extern void UnloadMediaStreamSource([MarshalAs(UnmanagedType.LPWStr)]string playerId);
#endif

        [DllImport("MediaEngineUWP", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "Play")]
        internal static extern void Play([MarshalAs(UnmanagedType.LPWStr)]string playerId);

        [DllImport("MediaEngineUWP", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "Pause")]
        internal static extern void Pause([MarshalAs(UnmanagedType.LPWStr)]string playerId);
    }
}
