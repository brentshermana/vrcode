using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace ZenFulcrum.EmbeddedBrowser {


/**
 * Provides a base implementation for interacting with a browser mesh "laser pointer style".
 * Assuming a Vive controller:
 *   Trigger is left click, grip is right click. Slide along the touchpad for a scroll wheel.
 * 
 * By default you'll get a vibrations on focus gain/loss, cursor to/from pointer, and scrolling.
 * 
 * Before you start fill {pointers} with tracked objects you wish to be capable of interacting with the browser.
 */
[RequireComponent(typeof(Browser))]
[RequireComponent(typeof(MeshCollider))]
public class VRPointerUI : MonoBehaviour, IBrowserUI {

	public SteamVR_TrackedObject[] pointers;

	public float maxDistance = float.PositiveInfinity;

	public bool enableVibrate = true;

	[Tooltip("How much we must slide a finger before we start scrolling (so we don't scroll when we just meant to click).")]
	public float scrollThreshold = .1f;

	[Tooltip(@"How far on the touchpad (which is 2 units wide and tall) we have to move our finger per mouse scroll click.
Set to a negative number to enable that infernal ""natural scrolling"" that's been making so many trackpads unusable lately.")]
	public float trackpadScrollSpeed = .05f;

	[Tooltip("How far each scroll \"tick\" scrolls. Normal mouse scroll wheels are usually 120.")]
	public int browserScrollSpeed = 20;

	[Tooltip("How far to keep the cursor from the surface of the browser. Set it as low as you can without causing z-fighting.")]
	public float cursorZStep = .005f;
	public float cursorScale = .1f;

	private SteamVR_TrackedObject activePointer;
	private MeshCollider meshCollider;
	private BrowserNative.CursorType lastCursorType;
	private bool touchIsScrolling;
	private Vector2 lastTouchPoint;

	private GameObject cursor, cursorImage;

	public virtual void Awake() {
		GetComponent<Browser>().UIHandler = this;
		meshCollider = GetComponent<MeshCollider>();

		InputSettings = new BrowserInputSettings();
		InputSettings.scrollSpeed = 10;

		BrowserCursor = new BrowserCursor();
		BrowserCursor.cursorChange += CursorChange;

		CreateCursor();
	}

	private void CreateCursor() {
		cursor = new GameObject("Cursor");
		cursor.transform.parent = transform;

		cursorImage = GameObject.CreatePrimitive(PrimitiveType.Quad);
		cursorImage.name = "Cursor Image";
		cursorImage.transform.parent = cursor.transform;
		var mr = cursorImage.GetComponent<MeshRenderer>();
		mr.sharedMaterial = Resources.Load<Material>("Browser/CursorDecal");
		Destroy(cursorImage.GetComponent<Collider>());

		cursor.transform.localScale = new Vector3(cursorScale, cursorScale, cursorScale);
		cursor.SetActive(false);
	}

	protected SteamVR_Controller.Device Input {
		get { return SteamVR_Controller.Input((int)activePointer.index); }
	} 

	public void InputUpdate() {
		RaycastHit hit;
		if (!activePointer) {
			hit = new RaycastHit();
			for (int i = 0; i < pointers.Length; i++) {
				if (Raycast(pointers[i], out hit)) {
					activePointer = pointers[i];
					PointerEnter();
					Shake();
					break;
				}
			}
		} else {
			Raycast(activePointer, out hit);
		}

		if (hit.transform != meshCollider.transform) {
			//not pointing at it.

			if (activePointer) {
				PointerLeave();
				activePointer = null;
			}

			MousePosition = new Vector3(0, 0);
			MouseButtons = 0;
			MouseScroll = new Vector2(0, 0);

			MouseHasFocus = false;
			KeyboardHasFocus = false;

			return;
		}

		MouseHasFocus = true;
		KeyboardHasFocus = true;
		MousePosition = hit.textureCoord;

		cursor.transform.position = hit.point + hit.normal * cursorZStep;
		cursor.transform.rotation = Quaternion.LookRotation(-hit.normal, transform.up);

		var input = SteamVR_Controller.Input((int)activePointer.index);
		ReadButtons(input);
		ReadScroll(input);
	}

	protected virtual void PointerEnter() {
		Shake();

		cursor.SetActive(true);

		var model = activePointer.gameObject.GetComponentInChildren<SteamVR_RenderModel>();
		if (model) {
			model.controllerModeState.bScrollWheelVisible = true;
		}
	}

	protected virtual void PointerLeave() {
		Shake();

		cursor.SetActive(false);

		var model = activePointer.gameObject.GetComponentInChildren<SteamVR_RenderModel>();
		if (model) {
			model.controllerModeState.bScrollWheelVisible = false;
		}
	}

	/**
	 * Reads the input from the given controller and turns it into mouse clicks.
	 * Override to change bindings as you see fit for your application.
	 */ 
	protected virtual void ReadButtons(SteamVR_Controller.Device input) {
		var buttons = (MouseButton)0;
        if (input.GetPress(EVRButtonId.k_EButton_SteamVR_Touchpad)) buttons |= MouseButton.Left;
//		if (input.GetPress(EVRButtonId.k_EButton_Grip)) buttons |= MouseButton.Right;
		MouseButtons = buttons;
	}

	/**
	 * Reads the "scroll wheel", which we read from the trackpad.
	 */
	protected virtual void ReadScroll(SteamVR_Controller.Device input) {
		var button = EVRButtonId.k_EButton_SteamVR_Touchpad;

		//Note: touchPoint/lastTouchPoint are in "expanded" space where one unit is one scroll tick.
		var touchPoint = input.GetAxis(button) / trackpadScrollSpeed;
		if (input.GetTouch(button) && !input.GetTouchDown(button)) {//if we didn't just put our finger down or up
			var delta = touchPoint - lastTouchPoint;
			if (!touchIsScrolling) {
				if (delta.magnitude * trackpadScrollSpeed > scrollThreshold) {
					touchIsScrolling = true;
					lastTouchPoint = touchPoint;
				} else {
					//don't start updating the touch point yet
				}
			} else {
				var quantizedDelta = new Vector2(Mathf.Round(delta.x), Mathf.Round(delta.y));
				MouseScroll = new Vector2(-quantizedDelta.x, quantizedDelta.y);
				InputSettings.scrollSpeed = browserScrollSpeed;

				var leftover = quantizedDelta - delta;
				//accumulate "unused" motion across ticks (slow finger sliding still scrolls normally)
				lastTouchPoint = touchPoint + leftover;
			}

		} else {
			MouseScroll = new Vector2();
			lastTouchPoint = touchPoint;
			touchIsScrolling = false;
		}
	}

	protected virtual bool Raycast(SteamVR_TrackedObject pointer, out RaycastHit hit) {
		if (!pointer.isValid) {
			hit = new RaycastHit();
			return false;
		}

		var ray = new Ray(pointer.transform.position, pointer.transform.forward);
		Physics.Raycast(ray, out hit, maxDistance);

		return hit.transform == meshCollider.transform;
	}

	protected virtual void Shake() {
		if (activePointer && enableVibrate) Input.TriggerHapticPulse();
	}

	protected virtual void CursorChange() {
		//if (lastCursorType == BrowserNative.CursorType.Hand || BrowserCursor.CursorType == BrowserNative.CursorType.Hand) {
		//	Shake();
		//}
		//lastCursorType = BrowserCursor.CursorType;

		var cursorRenderer = cursorImage.GetComponent<Renderer>();
		if (BrowserCursor.Texture) {
			cursorRenderer.enabled = true;
			cursorRenderer.material.mainTexture = BrowserCursor.Texture;

			var hs = BrowserCursor.Hotspot;
			cursorRenderer.transform.localPosition = new Vector3(
				.5f - hs.x / BrowserCursor.Texture.width, 
				-.5f + hs.y / BrowserCursor.Texture.height, 
				0
			);
		} else {
			cursorRenderer.enabled = false;
		}

	}


	public bool MouseHasFocus { get; private set; }
	public Vector2 MousePosition { get; private set; }
	public MouseButton MouseButtons { get; private set; }
	public Vector2 MouseScroll { get; private set; }
	public bool KeyboardHasFocus { get; private set; }
	public List<Event> KeyEvents { get; private set; }
	public BrowserCursor BrowserCursor { get; private set; }
	public BrowserInputSettings InputSettings { get; private set; }

}

}
