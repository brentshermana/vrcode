﻿using System;
using Leap.Unity.Interaction;
using UnityEngine;
using vrcode.ide.debugger.frontend;
using System.Collections.Generic;
using vrcode.networking.message;

namespace vrcode.ide.debugger.BreakPoints
{
    [RequireComponent(typeof(Rigidbody))]
    public class Breakpoint : InteractionBehaviour
    {
        // TODO: this information should not be tracked by breakpoint. Change this once there's a better idea
        //       of how to manage this
        //[SerializeField] private string SOURCE_FILENAME;
        
        
        // track all breakpoint positions
        private static Dictionary<int, Breakpoint> all_breakpoints;
        
        // determine BP positioning
        
        // define the starting 'slot' where breakpoints are placed,
        // and the distance between each one
        [SerializeField] private Vector3 origin;
        [SerializeField] private Vector3 inc;
        
        // maximum distance from a placement site a bp can be released in order
        // for that placement to be registered as valid
        [SerializeField] private float max_release_dist;
        
        // the number of placement sites
        [SerializeField] private int num_placement_sites;
        
        // Where the breakpoints come from
        [SerializeField] private Vector3 generation_site;
        [SerializeField] private Quaternion generation_rotation;

        [SerializeField] private DBFrontend debugger;

        [SerializeField] private float lerpFactor;

        private Rigidbody rb;
        
        // state variables
        private int line = -1;
        private Vector3 target_position;
        

        void Start()
        {
            if (all_breakpoints == null)
            {
                all_breakpoints = new Dictionary<int, Breakpoint>();
            }
            
            generation_site = transform.position;
            generation_rotation = transform.rotation;
            
            OnGraspBegin += onGraspBegin;
            OnGraspBegin += ReplaceSelf;
            
            OnGraspEnd += onGraspEnd;

            rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            if (line >= 0 && !isGrasped)
            {
                transform.position = Vector3.Lerp(transform.position, target_position, lerpFactor * Time.deltaTime);
            }
            else if (line < 0 && !isGrasped)
            {
                transform.position = Vector3.Lerp(transform.position, generation_site, lerpFactor * Time.deltaTime);
            }

            if (!isGrasped)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, lerpFactor * Time.deltaTime);
            }
        }

        void ReplaceSelf()
        {
            Instantiate(gameObject, generation_site, generation_rotation);
            OnGraspBegin -= ReplaceSelf;
        }

        void onGraspBegin()
        {
            if (line >= 0)
            {    
                all_breakpoints.Remove(line);
                debugger.ClearBreakpoint(TheEnvironment.GetSourceFilePath(), line, (RPCMessage msg, DebuggerError err) =>
                {
                    if (err != null)
                    {
                        UnityEngine.Debug.LogError(err);
                        // TODO: would also be a good idea to create another breakpoint and place it there
                        //       if one doesn't already exist at that location
                    }
                    else
                    {
                        UnityEngine.Debug.Log("Breakpoint: removed from line " + line);
                    }
                });
            }
        }

        void onGraspEnd()
        {
            Vector3 pos = origin;

            float lowest_dist = max_release_dist*max_release_dist + 1;
            int best_i = -1;
            
            for (int i = 0; i < num_placement_sites; i++)
            {
                
                float dist = (transform.position - pos).sqrMagnitude;
                if (dist < lowest_dist && !all_breakpoints.ContainsKey(i))
                {
                    best_i = i;
                    lowest_dist = dist;
                }

                pos += inc;
            }

            if (lowest_dist < max_release_dist * max_release_dist)
            {
                line = best_i + 1; // index 1 based, not zero
                UnityEngine.Debug.Log("Breakpoint set at line " + line);
                debugger.SetBreakpoint(TheEnvironment.GetSourceFilePath(), line, ((message, error) => { if (error != null) Destroy(gameObject); }));
                target_position = origin + inc * best_i;
                all_breakpoints[line] = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}