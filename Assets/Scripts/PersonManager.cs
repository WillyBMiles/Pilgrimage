using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PersonManager : MonoBehaviour
{
    [SerializeField]
    Person personPrefab;

    TentPlacementManager tpm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tpm = FindFirstObjectByType<TentPlacementManager>();
        tpm.OnPlace += OnNewTent;
        tpm.OnClearTent += OnClearTent;

        GameController.Instance.OnTransitionTo += OnTransitionTo;
    }

    private void OnTransitionTo(GameController.State state)
    {
        if (state == GameController.State.Placement)
        {
            StartDay();
        }
        if (state == GameController.State.TentSelection)
        {
            Leave();
        }
        if (state == GameController.State.Win)
        {
            Win();
        }
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void DestroyAllPeople()
    {
        foreach (Person p in allPeople)
        {
            if (p != null)
            {
                Destroy(p.gameObject);
            }
        }

        allPeople.Clear();
        currentPeople.Clear();
        nextPeople.Clear();
    }

    List<Person> allPeople = new();
    List<Person> currentPeople = new();
    List<Person> nextPeople = new();


    bool started = false;
    void StartDay()
    {
        started = false;
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(.1f);
        sequence.AppendCallback(() =>
        {
            try
            {
                
                Location currentLocation = GameController.Instance.CurrentLocation;

                foreach (var _ in TentPlacementManager.currentlyPlacing.TentBlocks)
                {
                    Person p = Instantiate(personPrefab, currentLocation.spawnArea.position, Quaternion.identity);
                    allPeople.Add(p);
                    nextPeople.Add(p);
                }

                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(.2f);
                sequence.AppendCallback(() =>
                {
                    MoveForward();
                    started = true;
                });
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        });

    }

    void OnNewTent(Tent tent)
    {
        int index = 0;
        Sequence sequence = DOTween.Sequence();
        if (!currentPeople.Any() || started)
        {
            
            sequence.AppendInterval(.5f);
        }

        sequence.AppendCallback(() =>
        {
            //Everyone goes to tent
            foreach (TentBlock block in tent.TentBlocks)
            {
                if (!currentPeople.Any())
                {
                    tent.actuallyPlaced = true;
                    break;
                }
                if (currentPeople.Count <= index)
                    continue;
                Person p = currentPeople[index];
                StartCoroutineFor(p, p.MoveToTentBlock(block, currentPeople.ToArray(), () => tent.actuallyPlaced = true));
                index++;
            }

            currentPeople.Clear();

            MoveForward();
        });
    }

    void OnClearTent(Tent tent)
    {
        foreach (Person person in currentPeople)
        {
            StartCoroutineFor(person, person.LeaveFromStaging(GameController.Instance.CurrentLocation));
        }

        currentPeople.Clear();
        MoveForward();
    }

    void MoveForward()
    {
        currentPeople.AddRange(nextPeople);

        Location currentLocation = GameController.Instance.CurrentLocation;

        //everyone goes to staging area
        foreach (var person in currentPeople)
        {
            StartCoroutineFor(person, person.MoveToCurrenStagingArea(currentLocation));
        }
        nextPeople.Clear();

        if (tpm.NumTentsLeft > 0)
        {
            for (int i =0; i < tpm.NextTent.NumTentBlocks; i++)
            {
                Person p = Instantiate(personPrefab, currentLocation.spawnArea.position, Quaternion.identity);
                allPeople.Add(p);
                nextPeople.Add(p);

                StartCoroutineFor(p, p.Enter(currentLocation));
            }

        }
    }

    void Leave()
    {
        foreach (Tent t in CurrentGrid.Instance.PlacedTents)
        {
            StartCoroutine(LeaveCoroutine(t));
        }
    }
    IEnumerator LeaveCoroutine(Tent tent)
    {
        if (tent == null)
            yield break;
        yield return new WaitForSeconds(UnityEngine.Random.value * 4f);
        foreach (TentBlock tb in tent.TentBlocks)
        {
            if (tb == null)
                yield break;
            Location l = GameController.Instance.CurrentLocation;
            Person p = Instantiate(personPrefab, tb.transform.position, Quaternion.identity);
            allPeople.Add(p);
            tent.actuallyPlaced = false;
            StartCoroutineFor(p, p.ExitWithTent(l,tb));

        }
    }

    void Win()
    {
        foreach (Tent t in CurrentGrid.Instance.PlacedTents)
        {
            StartCoroutine(WinCoroutine(t));
        }
    }

    IEnumerator WinCoroutine(Tent tent)
    {
        if (tent == null)
            yield break;
        yield return new WaitForSeconds(UnityEngine.Random.value * 4f);
        foreach (TentBlock tb in tent.TentBlocks)
        {
            if (tb == null)
                yield break;
            Location l = GameController.Instance.CurrentLocation;
            Person p = Instantiate(personPrefab, tb.transform.position, Quaternion.identity);
            allPeople.Add(p);
            StartCoroutineFor(p, p.Win());

        }
    }

    void StartCoroutineFor(Person person, IEnumerator coroutine)
    {
        person.StopAllCoroutines();
        person.StartCoroutine(coroutine);
    }
}
