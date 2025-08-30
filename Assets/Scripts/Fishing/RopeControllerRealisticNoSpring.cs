//Source: https://www.habrador.com/tutorials/rope/3-another-simplified-rope/

using System.Collections.Generic;
using UnityEngine;

//Simulate a rope with verlet integration and no springs

public class RopeControllerRealisticNoSpring : MonoBehaviour
{
    //Objects that will interact with the rope
    public Transform whatTheRopeIsConnectedTo;
    public Transform whatIsHangingFromTheRope;

    //Line renderer used to display the rope
    private LineRenderer lineRenderer;

    //A list with all rope section
    private List<RopeSection> allRopeSections = new List<RopeSection>();

    //Rope data
    private float ropeSectionLength = 0.5f;

    private Color c1 = Color.red;
    private Color c2 = Color.blue;

    private void Start()
    {
        //Init the line renderer we use to display the rope
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = c1;
        lineRenderer.endColor = c2;

        //Create the rope
        Vector3 ropeSectionPos = whatTheRopeIsConnectedTo.position;

        for (int i = 0; i < 15; i++)
        {
            allRopeSections.Add(new RopeSection(ropeSectionPos));

            ropeSectionPos.y -= ropeSectionLength;
        }
    }

    private void Update()
    {
        //Display the rope with the line renderer
        DisplayRope();

        // Let the bait move freely via its own physics (casting, water physics, etc.)
        // and the rope adapts to follow it without affecting the bait's rotation
    }

    private void FixedUpdate()
    {
        UpdateRopeSimulation();
    }

    private void UpdateRopeSimulation()
    {
        float t = Time.fixedDeltaTime;

        //Move the first section to what the rope is hanging from (stays still)
        RopeSection firstRopeSection = allRopeSections[0];
        firstRopeSection.pos = whatTheRopeIsConnectedTo.position;
        allRopeSections[0] = firstRopeSection;

        //Move the last section to follow the bait's actual position
        RopeSection lastRopeSection = allRopeSections[allRopeSections.Count - 1];
        lastRopeSection.pos = whatIsHangingFromTheRope.position;
        allRopeSections[allRopeSections.Count - 1] = lastRopeSection;

        //Move the middle rope sections with Verlet integration
        for (int i = 1; i < allRopeSections.Count - 1; i++)
        {
            RopeSection currentRopeSection = allRopeSections[i];

            //Calculate velocity this update with damping for more natural movement
            Vector3 vel = (currentRopeSection.pos - currentRopeSection.oldPos) * 0.98f;

            //Update the old position with the current position
            currentRopeSection.oldPos = currentRopeSection.pos;

            //Find the new position (no manual gravity since rigid body handles physics)
            currentRopeSection.pos += vel;

            //Add it back to the array
            allRopeSections[i] = currentRopeSection;
        }

        //Make sure the rope sections have the correct lengths (very light constraints)
        for (int i = 0; i < 2; i++)
        {
            ImplementMaximumStretch();
        }
    }

    //Make sure the rope sections have the correct lengths
    private void ImplementMaximumStretch()
    {
        for (int i = 0; i < allRopeSections.Count - 1; i++)
        {
            RopeSection topSection = allRopeSections[i];
            RopeSection bottomSection = allRopeSections[i + 1];

            //The distance between the sections
            float dist = (topSection.pos - bottomSection.pos).magnitude;

            //What's the stretch/compression
            float distError = Mathf.Abs(dist - ropeSectionLength);

            Vector3 changeDir = Vector3.zero;

            //Compress this sections
            if (dist > ropeSectionLength)
            {
                changeDir = (topSection.pos - bottomSection.pos).normalized;
            }
            //Extend this section
            else if (dist < ropeSectionLength)
            {
                changeDir = (bottomSection.pos - topSection.pos).normalized;
            }
            //Do nothing
            else
            {
                continue;
            }

            Vector3 change = changeDir * distError * 0.3f; // Very light constraint strength

            // Don't constrain the first section (connected point) or last section (bait)
            if (i != 0 && i != allRopeSections.Count - 2)
            {
                bottomSection.pos += change * 0.5f;
                allRopeSections[i + 1] = bottomSection;

                topSection.pos -= change * 0.5f;
                allRopeSections[i] = topSection;
            }
            // If it's the second-to-last segment, only adjust the middle section, not the bait
            else if (i == allRopeSections.Count - 2)
            {
                topSection.pos -= change * 0.8f;
                allRopeSections[i] = topSection;
            }
            // If it's the first segment, only adjust the second section, not the connection point
            else if (i == 0)
            {
                bottomSection.pos += change * 0.8f;
                allRopeSections[i + 1] = bottomSection;
            }
        }
    }

    //Display the rope with a line renderer
    private void DisplayRope()
    {
        float ropeWidth = 0.2f;

        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;

        //An array with all rope section positions
        Vector3[] positions = new Vector3[allRopeSections.Count];

        for (int i = 0; i < allRopeSections.Count; i++)
        {
            positions[i] = allRopeSections[i].pos;
        }

        lineRenderer.positionCount = positions.Length;

        lineRenderer.SetPositions(positions);
    }

    //A struct that will hold information about each rope section
    public struct RopeSection
    {
        public Vector3 pos;
        public Vector3 oldPos;

        //To write RopeSection.zero
        public static readonly RopeSection zero = new RopeSection(Vector3.zero);

        public RopeSection(Vector3 pos)
        {
            this.pos = pos;

            this.oldPos = pos;
        }
    }
}