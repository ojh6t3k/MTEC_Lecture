using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;


public class PlayerMovement : MonoBehaviour
{
	public Animator animator;
	public NavMeshAgent agent;
	public float turnSmoothing = 15f;
	public float speedDampTime = 0.1f;
	public float slowingSpeed = 0.175f;
	public float turnSpeedThreshold = 0.5f;

	private readonly int hashSpeedPara = Animator.StringToHash("Speed");
	private readonly int hashLocomotionTag = Animator.StringToHash("Locomotion");
	private const float navMeshSampleDistance = 4f;
	private const float stopDistanceProportion = 0.1f;

	private Interactable currentInteractable;
	private Vector3 destinationPosition;
	private bool handleInput = true;
	private WaitForSeconds inputHoldWait;


	void Start ()
	{
		agent.updateRotation = false;

		destinationPosition = transform.position;
	}

	void Update ()
	{
		if (agent.pathPending)
			return;
		
		float speed = agent.desiredVelocity.magnitude;
		if(agent.remainingDistance <= agent.stoppingDistance * stopDistanceProportion)
		{
			agent.Stop();

			transform.position = destinationPosition;

			speed = 0f;

			if (currentInteractable)
			{
				transform.rotation = currentInteractable.interactionLocation.rotation;

				currentInteractable.Interact();
				currentInteractable = null;

				StartCoroutine (WaitForInteraction ());
			}
		}
		else if(agent.remainingDistance <= agent.stoppingDistance)
		{
			agent.Stop();

			float proportionalDistance = 1f - agent.remainingDistance / agent.stoppingDistance;
			Quaternion targetRotation = transform.rotation;
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, proportionalDistance);
			transform.position = Vector3.MoveTowards(transform.position, destinationPosition, slowingSpeed * Time.deltaTime);

			speed = Mathf.Lerp(slowingSpeed, 0f, proportionalDistance);
		}
		else if(speed > turnSpeedThreshold)
		{
			Quaternion targetRotation = Quaternion.LookRotation(agent.desiredVelocity);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSmoothing * Time.deltaTime);
		}

		animator.SetFloat(hashSpeedPara, speed, speedDampTime, Time.deltaTime);
	}

	private void OnAnimatorMove()
	{
		agent.velocity = animator.deltaPosition / Time.deltaTime;
	}

	public void OnGroundClick(BaseEventData data)
	{
		if(!handleInput)
			return;

		currentInteractable = null;

		PointerEventData pData = (PointerEventData)data;

		NavMeshHit hit;
		if(NavMesh.SamplePosition(pData.pointerCurrentRaycast.worldPosition, out hit, navMeshSampleDistance, NavMesh.AllAreas))
			destinationPosition = hit.position;
		else
			destinationPosition = pData.pointerCurrentRaycast.worldPosition;

		agent.SetDestination(destinationPosition);
		agent.Resume ();
	}

	public void OnInteractableClick(Interactable interactable)
	{
		if(!handleInput)
			return;

		currentInteractable = interactable;

		destinationPosition = currentInteractable.interactionLocation.position;

		agent.SetDestination(destinationPosition);
		agent.Resume ();
	}


	private IEnumerator WaitForInteraction ()
	{
		handleInput = false;

		yield return inputHoldWait;

		while (animator.GetCurrentAnimatorStateInfo (0).tagHash != hashLocomotionTag)
		{
			yield return null;
		}

		handleInput = true;
	}
}
