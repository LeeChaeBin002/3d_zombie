using System.Collections;
using System.Drawing.Text;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum State
    {
        Ready,
        Empty,
        Reloading,
    }

    private State currentState = State.Ready;

    public State CurrentState
    {
        get { return currentState; }
        private set 
        {
            currentState = value;
            switch (currentState)
            {
                case State.Ready:
                    break;

                case State.Empty:
                    break;

                case State.Reloading:
                    break;

            }
        }
    }

    

    public GunData gunData;

    public ParticleSystem muzzleEffet;
    public ParticleSystem shellEffet;

    private LineRenderer lineRenderer;
    private AudioSource audioSource;

    public Transform firePosition;

    public int ammoRemain;
    public int magAmmo;

    private float lastFireTime;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.enabled = false;
        lineRenderer.positionCount = 2;

    }
    private void OnEnable()
    {
        ammoRemain = gunData.startAmmoRemain;
        magAmmo = gunData.magCapacity;

        lastFireTime = 0f;
        currentState = State.Ready;
    }
    private void Update()
    {
        switch(currentState)
        {
            case State.Ready:
                UpdateReady();
                break;
            case State.Empty:
                UpdateReady();
                break;
            case State.Reloading:
                UpdateReloading();
                break;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }
    }

    private void UpdateReady()
    {

    }
    private void UpdateEmpty()
    {

    }
    private void UpdateReloading()
    {

    }
    private IEnumerator CoShotEffet(Vector3 hitPosition)
    {
        audioSource.PlayOneShot(gunData.shootClip);

        muzzleEffet.Play();
        shellEffet.Play();
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, firePosition.position);
        Vector3 endPos = firePosition.position + firePosition.forward * 10f;

        lineRenderer.SetPosition(1, endPos);
        yield return new WaitForSeconds(1f);
        lineRenderer.enabled = false;
       
    }
    public void Fire()
    {

        if(magAmmo<=0)
        {
            currentState = State.Empty;
            return;
        }
        if(currentState==State.Ready&&Time.time>lastFireTime+gunData.timeBetFire)
        {
            lastFireTime = Time.time;
            Shoot();
        }
    }
    public void Shoot()
    {
        Vector3 hitPosition = Vector3.zero;
        RaycastHit hit;
        if(Physics.Raycast(firePosition.position,firePosition.forward,out hit,gunData.fireDistance))
        {
            hitPosition = hit.point;

            var target = hit.collider.GetComponent<IDamagable>();

            if(target!=null)
            {
                target.OnDamage(gunData.damage, hit.point, hit.normal);
            }
        }
        else
        {
            hitPosition = firePosition.position + firePosition.forward * gunData.fireDistance;
        }

        StartCoroutine(CoShotEffet(hitPosition));

        --magAmmo;
        if(magAmmo==0)
        {
            currentState = State.Empty;
        }
    }    
    public bool Reload()
    {
        if (currentState == State.Reloading||
            ammoRemain==0||
            magAmmo==gunData.magCapacity) 
            return false;
        currentState = State.Reloading;
        //if (magAmmo >= gunData.magCapacity) return false;
        //if (ammoRemain <= 0) return false;
        StartCoroutine(CoReload());
        return true;
    }

    private IEnumerator CoReload()
    {
        currentState = State.Reloading;
        
        audioSource.PlayOneShot(gunData.reloadClip);

        yield return new WaitForSeconds(gunData.reloadTime);


        // 예비탄에서 탄창으로 옮기기
        int need = gunData.magCapacity - magAmmo;
        int take = Mathf.Min(need, ammoRemain);

        magAmmo += take;
        ammoRemain -= take;

        // 다시 Ready로
        currentState = State.Ready;

    }
}
