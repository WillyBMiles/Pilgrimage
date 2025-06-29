using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class Person : MonoBehaviour
{
    [SerializeField]
    List<SpriteRenderer> shirtSprites;
    [SerializeField]
    List<SpriteRenderer> skinSprites;
    [SerializeField]
    List<SpriteRenderer> backpacks;

    [SerializeField]
    Gradient shirt;
    [SerializeField]
    Gradient skin;

    [SerializeField]
    Supply supplyPrefab;

    [SerializeField]
    GameObject puffOfSmoke;

    Supply mySupply;

    public bool wearingBackpack;

    [SerializeField]
    float speed;

    [SerializeField]
    Animator animator;

    SpriteRenderer[] allRenderers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        animator = GetComponent<Animator>();

        SetColors(shirt.Evaluate(UnityEngine.Random.value), skin.Evaluate(UnityEngine.Random.value));
    }


    public void SetColors(Color shirtMix, Color skinMix)
    {
        foreach (SpriteRenderer sr in shirtSprites)
        {
            sr.color = shirtMix;
        }
        foreach (SpriteRenderer sr in skinSprites)
        {
            sr.color = skinMix;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (wearingBackpack)
        {
            foreach (var item in backpacks)
            {
                item.enabled = true;
            }
        }
        else
        {
            foreach (var item in backpacks)
            {
                item.enabled = false;
            }
        }

        int sortingOrder = Mathf.RoundToInt(-transform.position.y * 10f);
        foreach (SpriteRenderer sr in allRenderers)
        {
            sr.sortingOrder = sortingOrder;
        }

        
    }

/*
 different situations:
    A) People walk in and drop their supplies to be the "next" 
        Each location has an "on deck" spot
        People spawned associated with tent
        Start off stage (location dependent)
        Get assigned a nearby spot to the on deck spot
            Walk in carrying backpacks
            Interact and drop supplies
            Wait
    B) Peple move their supplies to be the "current" tent
        Start from where they are
        Interact to get supply, put on backpack
        Walk to newly assigned spot (also location designated)
        Interact and drop supplies
        Wait
    C) People move their supplies to the actual map to place their map on the ground and then 
        Interact to pick up your supply, put on backpack
        Walk to newly assigned location
        Interact to place backpack
        Wait til ALL people in task have arrived
        All interact
        Plume of smoke
        Tent appears => people dissappear
    D) On the way out everyone picks up their tents and 
        Each tent has some delay to this
        Spawn NEW people at tent.
        Everyone interacts
        Puff of smoke
        Now it's supplies
        pause
        Everyone interacts to get a supply
        Everyone walks out
 
 */


    IEnumerator SetTarget(Vector3 target)
    {
        Vector2 start = transform.position;
        Vector2 end = target;
        Vector2 controlStart = (Vector2) (end + start) / 2 + UnityEngine.Random.insideUnitCircle;
        Vector2 controlEnd = (Vector2)(end + start) / 2 + UnityEngine.Random.insideUnitCircle;

        float distance = Vector2.Distance(start, end);

        float factor = 0f;
        animator.SetBool("Walking", true);
        bool walking = true;
        SoundManager.Play(SoundManager.Sound.Footsteps, () => 
            this == null || !walking);
        while (Vector2.SqrMagnitude(transform.position - target) > .01f)
        {
            factor += (speed * Time.deltaTime) / distance;
            transform.position = DOCurve.CubicBezier.GetPointOnSegment(start, controlStart, end, controlEnd, factor);
            PointTowards(target);
            yield return null;
        }
        walking = false;
        animator.SetBool("Walking", false);
    }

    void PointTowards(Vector3 position)
    {
        if (position.x < transform.position.x)
        {
            transform.localScale = new Vector3(1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1);
        }
    }

    private void OnDestroy()
    {
        if (mySupply != null)
        {
            Destroy(mySupply.gameObject);
        }
    }

    IEnumerator Interact()
    {
        animator.SetTrigger("Interact");
        yield return new WaitForSeconds(.5f);
    }

    IEnumerator PlaceSupply(Vector3 position)
    {
        PointTowards(position);
        
        yield return Interact();
        SoundManager.Play(SoundManager.Sound.putDown);
        if (mySupply == null)
        {
            mySupply = Instantiate(supplyPrefab);
        }
        mySupply.transform.position = position;
        mySupply.gameObject.SetActive(true);
        wearingBackpack = false;
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f, .5f));
    }

    IEnumerator PickupSupply()
    {
        if (mySupply != null)
        {
            PointTowards(mySupply.transform.position);
            
        }
           
       
        yield return Interact();
        if (mySupply != null)
        {
            mySupply.gameObject.SetActive(false);
            SoundManager.Play(SoundManager.Sound.pickUp);
        }
        wearingBackpack = true;
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f, .5f));
    }



    public IEnumerator Enter(Location location)
    {
        transform.position = location.spawnArea.position;
        wearingBackpack = true;

        yield return SetTarget(GetOffset(location.nextStagingArea.position));
        yield return PlaceSupply(GetOffset(transform.position));

    }

    public IEnumerator MoveToCurrenStagingArea(Location location)
    {
        yield return PickupSupply();
        yield return SetTarget(GetOffset(location.currentStagingArea.position));
        yield return PlaceSupply(GetOffset(transform.position));
    }

    public IEnumerator MoveToTentBlock(TentBlock tentBlock, Person[] allPeople, Action finished)
    {

        Vector2 tentPosition = (Vector2)tentBlock.transform.position + new Vector2(.5f, .5f);

        yield return PickupSupply();
        yield return SetTarget(tentPosition + UnityEngine.Random.insideUnitCircle * .2f);
        yield return PlaceSupply(tentPosition);

        yield return WaitUntilEveryoneIsReady(allPeople);

        yield return new WaitForSeconds(UnityEngine.Random.value / 5f);
        yield return Interact();

        yield return SpawnPuffOfSmoke(tentPosition);

        Destroy(gameObject);
        finished?.Invoke();
    }

    public IEnumerator ExitWithTent(Location location, TentBlock tentBlock)
    {
        Vector2 tentPosition = (Vector2)tentBlock.transform.position + new Vector2(.5f, .5f);
        yield return SpawnPuffOfSmoke(tentPosition);
        yield return PlaceSupply(tentPosition);
        yield return SetTarget(tentPosition + UnityEngine.Random.insideUnitCircle * .2f);
        yield return new WaitForSeconds(.5f);
        yield return PickupSupply();
        yield return SetTarget(location.exit.position);
        Destroy(gameObject);
    }

    public IEnumerator Win()
    {
        Vector3 startPos = transform.position;
        while (true)
        {
            if (UnityEngine.Random.value < .5f)
            {
                yield return Interact();
            } else
                yield return SetTarget((Vector2)startPos + UnityEngine.Random.insideUnitCircle * 5f);
            yield return new WaitForSeconds(UnityEngine.Random.value);
        }
        
    }

    public IEnumerator LeaveFromStaging(Location location)
    {
        yield return PickupSupply();
        yield return SetTarget(location.exitFromStaging.position);
        Destroy(gameObject);
    }

    bool readyToMoveOn = false;
    IEnumerator WaitUntilEveryoneIsReady(Person[] people)
    {
        readyToMoveOn = true;
        yield return new WaitUntil(() => people.All(p => p.readyToMoveOn));
    }

    IEnumerator SpawnPuffOfSmoke(Vector3 position)
    {
        Instantiate(puffOfSmoke, position, Quaternion.identity);
        SoundManager.Play(SoundManager.Sound.smoke);
        yield return new WaitForSeconds(.25f);
    }

    Vector2 GetOffset(Vector2 input)
    {
        return input + UnityEngine.Random.insideUnitCircle;
    }
}
