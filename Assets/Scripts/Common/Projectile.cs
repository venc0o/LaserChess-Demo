using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public Unit Target;
    public Unit Owner;

    public GameObject HitParticle;

    public float Speed = 5f;


	void Start ()
    {
        transform.parent = null;
	}
	

	void Update ()
    {

        transform.position = Vector3.Lerp(transform.position, transform.position + transform.forward, Time.deltaTime * Speed);


        if (Target != null && Vector3.Distance(Target.transform.position, transform.position) < 0.1f)
        {

            Target.GetDamage(Owner.AttackPower);

            if (HitParticle != null)
            {
                HitParticle.transform.parent = null;
                HitParticle.SetActive(true);
                Destroy(HitParticle, 3f);
            }            

            Destroy(this.gameObject);

        }

	}


}
