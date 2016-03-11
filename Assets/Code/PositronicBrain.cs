using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PositronicBrain : MonoBehaviour {

    public GameObject neuronSource;
    public GameObject synapsisSource;
    public GameObject emitter;
    protected ParticleSystem emitterParticleSystem;

    public int maxSynapsisPerNeuron = 5;
    public int neurons = 10;

    private List<Neuron> neuronList = new List<Neuron>();
    private List<Synapsis> synapsisList = new List<Synapsis>();
    private List<Message> messageList = new List<Message>();

    private float menuInfluence = 0f;

	// Use this for initialization
	void Start () {

        emitterParticleSystem = emitter.transform.FindChild("ParticleSystem").gameObject.GetComponent<ParticleSystem>();
        emitterParticleSystem.emissionRate = 0f;

        Neuron n = new Neuron(this);
        n.root.transform.localPosition = new Vector3(0f, 0f, 0f);
        n.SetOriginal();
        neuronList.Add(n);

        while (neuronList.Count < neurons)
        {
            Neuron randomNeuron = neuronList[Random.Range(0, neuronList.Count)];
            if (randomNeuron.synapsisList.Count < maxSynapsisPerNeuron)
            {
                Neuron n2 = new Neuron(this);
                Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                randomDirection.Normalize();
                float distance = Random.Range(55f, 60f);
                n2.root.transform.position = randomNeuron.root.transform.position + randomDirection * distance;
                n2.SetOriginal();

                float radius = 60f;
                int neighbours = 0;

                for (int i = 0; i < neuronList.Count; i++)
                {
                    if (Vector3.Distance(neuronList[i].root.transform.position, n2.root.transform.position) <= radius)
                    {
                        neighbours++;
                    }
                }

                if (neighbours < 10)
                {
                    neuronList.Add(n2);

                    Synapsis s = new Synapsis(this);
                    s.start = randomNeuron;
                    s.end = n2;
                    synapsisList.Add(s);
                    s.Update();

                    randomNeuron.synapsisList.Add(s);
                    n2.synapsisList.Add(s);

                    for (int j = 0; j < neuronList.Count; j++)
                    {
                        if (neuronList[j] != n2 && !n2.SynapsisListHasNeuron(neuronList[j]) && Vector3.Distance(neuronList[j].root.transform.position, n2.root.transform.position) <= 17f)
                        {
                            Synapsis s2 = new Synapsis(this);
                            s2.start = n2;
                            s2.end = neuronList[j];
                            synapsisList.Add(s2);
                            s2.Update();

                            neuronList[j].synapsisList.Add(s2);
                            n2.synapsisList.Add(s2);
                        }
                    }
                    
                }
                else
                {
                    Destroy(n2.root);
                    n2 = null;
                }

            }
        }

        for (int i = 0; i < 10; i++)
        {
            Message m = new Message(this);
            m.targetNeuron = neuronList[0];
            messageList.Add(m);
        }
        
	
	}
	
	// Update is called once per frame
	void Update () {

        this.transform.Rotate(new Vector3(0f, Time.deltaTime*2.5f, 0f));

        this.transform.Rotate(new Vector3(0f, 1f, 0f) * menuInfluence * Time.deltaTime * 60f);
        menuInfluence = Mathf.Lerp(menuInfluence, 0f, Time.deltaTime*5f);

	}

    void FixedUpdate()
    {
        for (int i = 0; i < neuronList.Count; i++)
        {
            neuronList[i].Update();
        }

        for (int i = 0; i < synapsisList.Count; i++)
        {
            synapsisList[i].Update();
        }

        for (int i = 0; i < messageList.Count; i++)
        {
            messageList[i].Update();
        }
    }

    public void MenuForward()
    {
        menuInfluence = 1f;
    }

    public void MenuBack()
    {
        menuInfluence = -1f;
    }

    public class Neuron
    {

        public GameObject root;
        private Vector3 originalPosition;
        public List<Synapsis> synapsisList = new List<Synapsis>();
        private Vector3 newPosition = new Vector3(0f, 0f, 0f);
        private float cooldown = 0f;
        private float interpolationNumber = 0f;
        private Vector3 randomRotation;

        public Neuron(PositronicBrain pB)
        {
            root = GameObject.Instantiate(pB.neuronSource);
            root.SetActive(true);
            root.transform.SetParent(pB.transform);
            randomRotation = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            randomRotation.Normalize();
        }

        public void SetOriginal()
        {
            originalPosition = root.transform.localPosition;
        }

        public bool SynapsisListHasNeuron(Neuron n)
        {
            for (int i = 0; i < synapsisList.Count; i++)
            {
                if (synapsisList[i].HasNeuron(n))
                {
                    return true;
                }
            }
            return false;
        }

        public void Update()
        {
            root.transform.Rotate(randomRotation * Time.deltaTime *180f);

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

            root.transform.localPosition = Vector3.Lerp(root.transform.localPosition, newPosition, Time.deltaTime*interpolationNumber);
        }

    }

    public class Synapsis
    {

        public GameObject root;
        public GameObject cylinder;
        public Neuron start;
        public Neuron end;

        public Synapsis(PositronicBrain pB)
        {
            root = GameObject.Instantiate(pB.synapsisSource);
            root.SetActive(true);
            root.transform.SetParent(pB.transform);
            cylinder = root.transform.FindChild("Cylinder").gameObject;
        }

        public bool HasNeuron(Neuron n)
        {
            return (n == start || n == end);
        }

        public void Destroy()
        {
            GameObject.Destroy(root);
            start.synapsisList.Remove(this);
            end.synapsisList.Remove(this);
        }

        public void Update()
        {
            root.transform.position = (start.root.transform.position + end.root.transform.position)/2f;
            root.transform.LookAt(end.root.transform);
            float distance = Vector3.Distance(start.root.transform.position, end.root.transform.position);
            cylinder.transform.localScale = new Vector3(cylinder.transform.localScale.x, distance*0.5f, cylinder.transform.localScale.z);
        }

    }

    public class Message
    {

        private PositronicBrain parent;
        public GameObject root;
        public Neuron targetNeuron;

        public Message(PositronicBrain pB)
        {
            parent = pB;
            root = GameObject.Instantiate(pB.neuronSource);
            root.SetActive(true);
            root.transform.SetParent(pB.transform);
            //root.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.5f, 0.5f);
            root.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
        }

        public void Update()
        {
            if (Vector3.Distance(root.transform.position, targetNeuron.root.transform.position) < 1f)
            {
                if (Random.Range(0f, 100f) < 25f)
                {
                    parent.emitterParticleSystem.gameObject.transform.position = targetNeuron.root.transform.position;
                    parent.emitterParticleSystem.Emit(1);
                }
                Neuron randomNeuron = parent.neuronList[Random.Range(0, parent.neuronList.Count)];
                root.transform.position = randomNeuron.root.transform.position;
                Synapsis s = randomNeuron.synapsisList[Random.Range(0, randomNeuron.synapsisList.Count)];
                if (s.start == randomNeuron)
                {
                    targetNeuron = s.end;
                }
                else
                {
                    targetNeuron = s.start;
                }
            }
            root.transform.position = Vector3.Lerp(root.transform.position, targetNeuron.root.transform.position, Time.deltaTime * 10f);
        }

    }

}
