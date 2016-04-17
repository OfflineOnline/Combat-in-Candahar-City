using UnityEngine;
using System.Collections;

public class FreeCameraMovement : MonoBehaviour
{
	/*
	 * Dises Skript steuert die freie Kamera sobald es aktiv ist.
	 */

	public float speed = 1f;
	private float h, v;
	bool fast = false;
	private MouseLook mouseLook;

	void Start()
	{
		enabled = false;
		mouseLook = GetComponent<MouseLook>();
	}

	void Update()
	{
		// Input registrieren
		h = Input.GetAxisRaw("Horizontal");
		v = Input.GetAxisRaw("Vertical");

		// Diagonales Bewegen
		if (h != 0 && v != 0)
		{
			h *= 0.7071f;
			v *= 0.7071f;
		}

		transform.Translate(new Vector3(h, 0f, v) * speed);
	}

	public void Initialise()
	{
		enabled = true;
		mouseLook.tr_horizontal = transform;
		mouseLook.tr_vertical = transform;
	}
}