
This was a group  project that produced at my time at univeristy that covered various different methods when it comes to designing AI for simulation and games and secured a 93% for the entire group. 
This projects was produced in unity using c#.


Project outline:
We were tasked with building multiple different AI systems that would control a tank who's capabilities were predefined within a base abstract class of which we would need to inherit from 
and build our AI systems.These AI systems indcluded and were not limited to, finite state machines, rule based systems, and more advanced methods such as behaviour trees along with 
combinations of these systems for higher marks. The moudle also had a heavy focus on code reusability and flexbility so the systems we designed had a large focus on abstraction and the use of base classes 
for example all of the states for the state machine system had a base state class that inlcudeed methods like start, update and exit all of which were abstract methods allowing us to build our own states through 
inherting from the base state class while being able to easily plug them into the main state machine system due to all the states deriving from one main abstract state class.  

We also had a list of particualr behaviours that we would need to implement within these systems from behaviours like retreat, chase, attack, and search all of which had to be included within the 
systems we designed along with needing to define our own behaviours to push our systems further, requiring us to carefully consider the scenario that we were applying our AI systems to ensuring that 
our desgin for the behaviours aligned with what was needed and that they performed well as we were also judged based on how functional the behaviours for all of the AI systems we implmented were along with
how they performed.

The AI systems we implmented as a group: 

Finite state machine - this particualr system had an overarching state machine class that would be responsible for updating the current state and swaping between states, in this case all states 
contained singular behaviours with each state being a particualr class that would inherit from the base abstarct class for states allowing them to be easily inserted into the main state machine to be 
updated. This system also included unique behaviours aside from the ones outlined, those being wait,ambush,and dodge adding greater complexity and more expansion to the state machine beyond what was given. 
This AI system became the foundation for our other more complicated systems.

Finite state machine + rule based system - this particualr system took a more global approach to state switching with there being particualr global rules defined that would control when states would 
switch rather than the states having to define when they needed to switch, these rules consisted of various booleans known as stats that that represented the current condtion of the tank. All of these rules 
and booleans were define in a singualr place allowing for much better control over when the states would switch and made it easier to add conditions for state switching as all it required as an addtion of new stats 
that goverened a particualr rule. Each rule was an object of a rule class that held the particualr boolean oppertaion the rule was using, which state to return, and the stats that would be inlcuded within the rule. 
This meant that in order for the states to switch all that would be required would be to loop through the defined rules within the state and return a new state if they were hit. Rather than having a large amount of condtions for switching within the state 
the state could just contain the functionaility of the behaviour.  

FSM + RBS + behaviour tree -  This system was desgined to combine the more complex behaviours that could be generated with a behaviour tree system with the previous system with the behaviour tree system 
allowing us to define multiple indivual actions that would take place in sequence that would all have there own indidvual state such as success, failure, etc and these actions would often be related to a particualr operation 
that allowed us to define when the particualr state that a sequnce of actions or indivual action was associated with should switch by checking its rules(as this system also used the RBS). This system allowed for 
tighter control over each indivual action of the AI tank and allowed us to better expand our current behaviours defined in the previous systems above. 









