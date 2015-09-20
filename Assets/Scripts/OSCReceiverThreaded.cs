using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Ventuz.OSC;

public class OSCReceiverThreaded : MonoBehaviour {

	public int listeningPort;
	public UdpReader oscReceiver;
	private List<OscElement> receivedMessages;

	// read Thread
	Thread receiveThread;

	private System.Object lockObject;

	// Use this for initialization
	void Start () {

		this.lockObject = new System.Object ();
		this.oscReceiver = null;
		this.receivedMessages = null;
		this.receivedMessages = new List<OscElement> ();
		Debug.Log("Initializing Receiver " );
		if (oscReceiver == null)
		{
			oscReceiver = new UdpReader(listeningPort);
		}

		Debug.Log("Initializing thread " );

		//initializing thread
		receiveThread = new Thread(
			new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();

	}
	
	// Update is called once per frame
	void Update () {
		// check button "q" to abort the read-thread
		if (Input.GetKeyDown("q"))
			stopThread();
	}

	// Unity Application Quit Function
	void OnApplicationQuit()
	{
		stopThread();
	}

	// Stop reading UDP messages
	private void stopThread()
	{
		if (receiveThread.IsAlive)
		{
			receiveThread.Abort();
		}
	}

	// receive thread
	private  void ReceiveData(){

		while (true)
		{
			try
			{
				receiveOSC();
			}
			catch (Exception err)
			{
				print(err.ToString());
			}
		}

	}
	
	private void receiveOSC(){

		OscMessage message = null;

		message = this.oscReceiver.Receive();
		
		// Return if there are no more messages available
		if (message != null) {
			if(message.GetType() == typeof(Ventuz.OSC.OscElement)){
				Debug.Log("is OscElement");
				if (Monitor.TryEnter (this.lockObject)) {
					receivedMessages.Add((OscElement) message);
					Monitor.Exit(lockObject);
				}
			}
			else{
				//we try to parse a bundle
				OscBundle bundle = message as OscBundle;
				if (bundle != null) {
					Debug.Log("is Bundle");
					// Enumerate over all elements
					IEnumerator e = bundle.Elements.GetEnumerator();
					while (e.MoveNext())
					{
						// Check if element matches OSC path of this gameObject
						OscElement el = e.Current as OscElement;
						Debug.Log("got OSC message! " + el.Address);


						if (Monitor.TryEnter (this.lockObject)) {
							receivedMessages.Add(el);
							Monitor.Exit(lockObject);
						}

					}
				}
			}
		}
	}
	
	public OscElement getFirstElementInQueue(){
		OscElement res = null;

		if (Monitor.TryEnter (this.lockObject)) {
			if (this.receivedMessages != null) {
				if (receivedMessages.Count > 0) {
					res = receivedMessages [0];
					receivedMessages.RemoveAt (0);
				}
			}
			Monitor.Exit(lockObject);
		}
		
		return res;
	}


}
