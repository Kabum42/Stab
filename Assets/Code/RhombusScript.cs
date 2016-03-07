using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RhombusScript : MonoBehaviour {

    public GameObject neuronSource;
    public GameObject synapsisSource;

    public GameObject boneNW;
    public GameObject boneNE;
    public GameObject boneSW;
    public GameObject boneSE;

    private Neuron NW;
    private Neuron NE;
    private Neuron SW;
    private Neuron SE;

    private List<Synapsis> synapsisList = new List<Synapsis>();

	// Use this for initialization
	void Start () {

        NW = new Neuron(this);
        NW.root.name = "Neuron_NW";
        NW.root.transform.localPosition = new Vector3(2f, 2f, 0f);
        NW.SetOriginal();

        NE = new Neuron(this);
        NE.root.name = "Neuron_NE";
        NE.root.transform.localPosition = new Vector3(-2f, 2f, 0f);
        NE.SetOriginal();

        SW = new Neuron(this);
        SW.root.name = "Neuron_SW";
        SW.root.transform.localPosition = new Vector3(2f, -2f, 0f);
        SW.SetOriginal();

        SE = new Neuron(this);
        SE.root.name = "Neuron_SE";
        SE.root.transform.localPosition = new Vector3(-2f, -2f, 0f);
        SE.SetOriginal();

        Synapsis s1 = new Synapsis(this);
        s1.start = NW;
        s1.end = NE;
        synapsisList.Add(s1);

        Synapsis s2 = new Synapsis(this);
        s2.start = NE;
        s2.end = SE;
        synapsisList.Add(s2);

        Synapsis s3 = new Synapsis(this);
        s3.start = SE;
        s3.end = SW;
        synapsisList.Add(s3);

        Synapsis s4 = new Synapsis(this);
        s4.start = SW;
        s4.end = NW;
        synapsisList.Add(s4);
	
	}
	
	// Update is called once per frame
	void Update () {

        for (int i = 0; i < synapsisList.Count; i++)
        {
            synapsisList[i].Update();
        }

        boneNW.transform.position = NW.root.transform.position;
        boneNE.transform.position = NE.root.transform.position;
        boneSW.transform.position = SW.root.transform.position;
        boneSE.transform.position = SE.root.transform.position;

	}

    public class Neuron
    {

        public GameObject root;
        private Vector3 originalPosition;
        private Vector3 newPosition = new Vector3(0f, 0f, 0f);
        private float cooldown = 0f;
        private float interpolationNumber = 0f;

        public Neuron(RhombusScript rS)
        {
            root = GameObject.Instantiate(rS.neuronSource);
            root.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            root.SetActive(true);
            root.transform.SetParent(rS.transform);
        }

        public void SetOriginal()
        {
            originalPosition = root.transform.localPosition;
        }

        public void Update()
        {
            cooldown -= Time.deltaTime;

            if (cooldown <= 0f)
            {
                cooldown = Random.Range(0.5f, 1f);
                Vector3 direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                direction.Normalize();
                float distance = Random.Range(2f, 5f);
                newPosition = root.transform.localPosition + direction * distance;
                if (Vector3.Distance(newPosition, originalPosition) > 20f)
                {
                    Vector3 direction2 = (newPosition - originalPosition).normalized;
                    newPosition = originalPosition + direction2 * 20f;
                }
                interpolationNumber = Mathf.Pow(Random.Range(1f, 2f), 4f);
            }

            root.transform.localPosition = Vector3.Lerp(root.transform.localPosition, newPosition, Time.deltaTime * interpolationNumber);
        }

    }

    public class Synapsis
    {

        public GameObject root;
        public GameObject cylinder;
        public Neuron start;
        public Neuron end;

        public Synapsis(RhombusScript rS)
        {
            root = GameObject.Instantiate(rS.synapsisSource);
            root.SetActive(true);
            root.transform.SetParent(rS.transform);
            cylinder = root.transform.FindChild("Cylinder").gameObject;
            cylinder.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
        }

        public bool HasNeuron(Neuron n)
        {
            return (n == start || n == end);
        }

        public void Destroy()
        {
            GameObject.Destroy(root);
        }

        public void Update()
        {
            root.transform.position = (start.root.transform.position + end.root.transform.position) / 2f;
            root.transform.LookAt(end.root.transform);
            float distance = Vector3.Distance(start.root.transform.position, end.root.transform.position);
            cylinder.transform.localScale = new Vector3(cylinder.transform.localScale.x, distance * 0.5f, cylinder.transform.localScale.z);
        }

    }
}
