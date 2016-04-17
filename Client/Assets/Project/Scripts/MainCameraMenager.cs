using UnityEngine;
using System.Collections;

public class MainCameraMenager : MonoBehaviour
{
	/*
	 * Die Position der MainCamera wird nie direkt angesteuert. Die MainCamera folgt immer einem "Empty GameObject"
	 * und übernimmt dessen Position und Rotation.
	 * Dafür sorgt dieses Skript.
	 * Das Wechseln der Kameraansichten wird hier gesteuert.
	 * Mit dem Property -SpectatorMode- wird zwischen Game- und Spectatormodus gewechselt.
	 */

	private MouseLook mouseLook;			// Refferrenz auf das "Umgucken-Skript"
	private int currentCam = 0;				// Ausgewählte Sicht
	private Transform myCam;				// Eigene Ego-Sicht
	private Transform[] otherCam;			// Spectator-Sichten (andere Spieler)
	private FreeCameraMovement movement;	// Referenz auf die "Freie Kamera"-Steuerung
	private bool spectatorMode = false;		// Kann zwischen den Views hin- und herschalten
	private bool notPressed = false;		// Hilfsvariable zum Umschalten der Kamera

	public bool SpectatorMode
	{
		get { return spectatorMode; }
		set
		{
			spectatorMode = value;
			if (!spectatorMode) CurrentCam = 0;		// Spieler aus dem Spectatormodus holen
			else CurrentCam = 1;					// Kamera direkt auf den ersten Spieler setzen
		}
	}

	public int CurrentCam
	{
		get { return currentCam; }
		set
		{
			currentCam = SwitchCamera(value);
			if (currentCam == -1) movement.Initialise();	// Freie Kamerabewegung aktivieren
			else movement.enabled = false;					// Freie Kamerabewegung sperren
		}
	}

	void Start()
	{
		// Initialisierungen
		mouseLook = GetComponent<MouseLook>();
		myCam = GameObject.Find("PlayerCameraPos").transform;
		movement = GetComponent<FreeCameraMovement>();
		UpdateCams();
	}

	void Update()
	{
		// Testzeile (Mit "H" in den Spectatormodus wechseln)
		if (Input.GetKeyDown(KeyCode.H)) SpectatorMode = !SpectatorMode;

		// Steuerung im Spectatormodus
		if (SpectatorMode)
		{
			// Input abfangen
			int inp = (int)Input.GetAxisRaw("Horizontal");
			if (inp == 0) inp = (int)Input.GetAxisRaw("Vertical");
			bool free = Input.GetButtonDown("Fire1");

			if (currentCam != -1)
			{
				if (free) CurrentCam = -1;
				else
				{
					// 3rd-Person-Sicht wechseln
					if (notPressed && inp != 0)
					{
						if (inp > 0) CurrentCam = NextCamera(true);
						else CurrentCam = NextCamera(false);
						notPressed = false;
					}
					else if (inp == 0)
					{
						// Erneutes Wechseln der Sicht verhindern, wenn die Taste dazu noch nicht losgelassen wurde
						notPressed = true;
					}
				}
			}
			else
			{
				// Zur 3rd-Person-Sicht wechseln
				if (free) CurrentCam = NextCamera(true);
			}
		}

		if (CurrentCam == 0 && (myCam == null || myCam.gameObject.activeSelf == false)) CurrentCam = NextCamera(true);

		// MainKamera an Position "kleben"
		if (CurrentCam == 0) // Eigene Ego-Sicht
		{
			transform.position = myCam.position;
			transform.rotation = myCam.rotation;
		}
		else if (Available(CurrentCam))	// Spectator / 3rd-Person-Sicht
		{
			transform.position = otherCam[currentCam - 1].position;
			transform.rotation = otherCam[currentCam - 1].rotation;
		}
	}

	// Alle Spectatoransichten finden
	private void UpdateCams()
	{
		GameObject[] x = GameObject.FindGameObjectsWithTag("OnlinePlayerCam");
		otherCam = new Transform[x.Length];
		for(int i = 0; i < otherCam.Length; i++) otherCam[i] = x[i].transform;
	}

	// Gibt es diese Spectatoransicht?
	private bool Available(int cam)
	{
		try { return otherCam[cam - 1] != null; }
		catch { return false; }
	}

	// Nächste verfügbare Spectatoransicht finden
	private int NextCamera(bool next)
	{
		int help = CurrentCam;
		for(int i = 1; i <= otherCam.Length; i++)
		{
			if (next) // nächste Position
			{
				help++;
				if (help > otherCam.Length || help < 1) help = 1;
			}
			else // vorherige Position
			{
				help--;
				if (help < 1 || help > otherCam.Length) help = otherCam.Length;
			}
			if (Available(help)) // Position prüfen
			{
				return help;
			}
		}
		// freie Kamera zurückgeben, wenn kein anderer Spieler gefunden wurde
		return -1;
	}

	// Ungültige Werte abfangen und dem "Umgucken-Skript" sagen, welche Objekte es steuern soll
	private int SwitchCamera(int cam)
	{
		if (cam == 0 || !SpectatorMode)
		{
			// Wieder auf Ego-Sicht stellen
			mouseLook.PlayerPos();
			return 0;
		}
		else if (cam == -1)
		{
			return -1;
		}
		else if (Available(cam))
		{
			// Auf Spectator stellen wenn der Spieler verfügbar ist
			mouseLook.tr_horizontal = otherCam[cam - 1].parent;
			mouseLook.tr_vertical = otherCam[cam - 1].parent;
			return cam;
		}
		else
		{
			// Wenn der Spieler nicht verfügbar ist, dann einen anderen finden
			int x = NextCamera(true);
			if (x != -1)
			{
				mouseLook.tr_horizontal = otherCam[x - 1].parent;
				mouseLook.tr_vertical = otherCam[x - 1].parent;
			}
			return x;
		}
	}
}