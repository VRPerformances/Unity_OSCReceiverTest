using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Ventuz.OSC;

public class OSCReceiver : MonoBehaviour {

	public int listeningPort;
	public UdpReader oscReceiver;
	private List<OscElement> receivedMessages;

	// Use this for initialization
	void Start () {

		this.oscReceiver = null;
		this.receivedMessages = null;
		this.receivedMessages = new List<OscElement> ();

		if (oscReceiver == null)
		{
			oscReceiver = new UdpReader(listeningPort);
		}

	}
	
	// Update is called once per frame
	void Update () {
		this.receiveOSC ();
	}

	private void receiveOSC(){

		OscMessage message = this.oscReceiver.Receive();
		
		// Return if there are no more messages available
		if (message != null) {

			OscBundle bundle = message as OscBundle;
			if (bundle != null) {

				// Enumerate over all elements
				IEnumerator e = bundle.Elements.GetEnumerator();
				while (e.MoveNext())
				{
					// Check if element matches OSC path of this gameObject
					OscElement el = e.Current as OscElement;
					Debug.Log("got OSC message! " + el.Address);

					receivedMessages.Add(el);

				}
			}
		}
	}

	public OscElement getFirstElementInQueue(){
		OscElement res = null;

		if (this.receivedMessages != null) {
			if(receivedMessages.Count>0){
				res = receivedMessages[0];
				receivedMessages.RemoveAt(0);
			}
		}

		return res;
	}
}
