# HS2TakeChargePlugin

This plugin initializes, enables and completes a mostly there framework Illusion had in the game to enable a female take charge mode and an auto play mode.
\
Usage: Inside a Main Game only HScene, press Ctrl+T (default, rebind in Plugin Settings if desired)...then sit back and watch.
\
Note:\
When you enable the mode, the female char selects a 'Female Dominate' position from POSITIONS AVAILABLE TO HER. This depends on map and state.
A new girl will have access to few or none of these. If none are available it'll just autoplay-loop on the current scene. 
\
This should only really be used with girls with more advanced experience/state. I don't believe slave girls have access to many dominant positions either, but haven't tested it.

The Ctrl+A (default) auto mode engages, well, auto mode - similar to above. Game plays itself essentially until you turn it off or the female exhausts. It will randomly select from all available positions, but is limited to Caress, Service and Sex positions (including the Women led versions). Special, Lesbian and Multichar modes will not be selected and enabling auto mode inside of them doesn't entirely function.

Auto Animation Timing: When turned on (in plugin settings) overrides the animation speed with a (configurable - see Rules below) timing pattern. Adds a bit of variety to the visuals. Note - this doesn't change the arousal meter speed, just the visuals. If you enable auto speed in all modes, you still have control of the arousal speed and changing between slow/fast loops with the mouse wheel - it just won't change the animation speed.

Manual Speed Control Hotkeys - These do the same thing as the auto animation timing stuff, but give you manual control of the animation visual speed. Like the auto animation, this doesn't change the arousal meter speed, instead it just adds a visual bump (or decrease) to how fast the animation visually appears. The increase or decrease is added or subtracted from the mouse wheel speed. So the mouse wheel still works to increase/decrease the speed and switch between slow/fast loops, then this is applied on top.

Lots of configuration options in the plugin settings menu, but for detailed control, use the new Rules feature. A rule triggers for criteria that match the header of the rule and allows you to specify Girls includes/excluded for the given Animation or Category, Rooms to include/exclude and Timing for the new auto animation timing overrides.

When you first load the plugin, head into the game and do one H scene (well, really you just need to go to lobby and pick a girl) and the plugin will write a default HS2TakeChargeRules.xml to the (default) UserData directory. Open this up and check the comments at the bottom for detailed information of how to write rules, notably a complete dump of the right animation names and room names you need to use in the rules.

A rule looks like this:
```
<TCRule Rule="SERVICE" RuleType="CATEGORY" ExcludeAlways="false">
      <CharacterApplication>
        <Includes>
          <Include>Susan</Include>
        </Includes>
        <Excludes>
          <Exclude>Mary</Exclude>
        </Excludes>
      </CharacterApplication>
      <RoomApplication>
        <Includes />
        <Excludes>
          <Exclude>[2155X] PH Subway</Exclude>
        </Excludes>
      </RoomApplication>
      <AnimationTimings>
        <AnimationTiming Stage="IDLE" MinSpeed="0" MaxSpeed="0" SpeedLoopTime="1" SpeedEase="Linear" LoopType="Yoyo" MinFemaleOffset="0" MaxFemaleOffset="0" FemaleOffsetLoopTime="1" FemaleOffsetEase="Linear" />
        <AnimationTiming Stage="SLOW_LOOP" MinSpeed="0.75" MaxSpeed="1.4" SpeedLoopTime="2.5" SpeedEase="InOutCubic" LoopType="Yoyo" MinFemaleOffset="-0.1" MaxFemaleOffset="0.1" FemaleOffsetLoopTime="1.5" FemaleOffsetEase="InOutCubic" />
        <AnimationTiming Stage="FAST_LOOP" MinSpeed="1.5" MaxSpeed="2.25" SpeedLoopTime="2.5" SpeedEase="InOutCubic" LoopType="Yoyo" MinFemaleOffset="-0.1" MaxFemaleOffset="0.1" FemaleOffsetLoopTime="1.5" FemaleOffsetEase="InOutCubic" />
        <AnimationTiming Stage="O_LOOP" MinSpeed="1.5" MaxSpeed="2.25" SpeedLoopTime="2.5" SpeedEase="InOutCubic" LoopType="Yoyo" MinFemaleOffset="-0.1" MaxFemaleOffset="0.1" FemaleOffsetLoopTime="1.5" FemaleOffsetEase="InOutCubic" />
        <AnimationTiming Stage="ORGASM" MinSpeed="0" MaxSpeed="0" SpeedLoopTime="1" SpeedEase="Linear" LoopType="Yoyo" MinFemaleOffset="0" MaxFemaleOffset="0" FemaleOffsetLoopTime="1" FemaleOffsetEase="Linear" />
        <AnimationTiming Stage="POST_ORGASM" MinSpeed="0" MaxSpeed="0" SpeedLoopTime="1" SpeedEase="Linear" LoopType="Yoyo" MinFemaleOffset="0" MaxFemaleOffset="0" FemaleOffsetLoopTime="1" FemaleOffsetEase="Linear" />
      </AnimationTimings>      
    </TCRule>
```    
    
This rule adds all service positions to Susan as Includes, meaning Susan will only pick from positions Included for her - so unless she has other Includes only Service positions will be selected. Mary is excluded from all Service positions, thus she will only pick other categories. Likewise if you are on the PH Subway map, no Service positions wil be selected.

Excludes override includes, the Excluded category (using the ExcludeAlways attribute) will always lock that out, even from characters/rooms marked include. ANIMATION rules override CATEGORY rules, so excluding a girl from a category but including specific animations works. Same with rooms.

If a girl or room has any Inclusions, these are the only options selected from. Otherwise, everything not specifically excluded is fair game.

Animation timing notes:

Stage - Applies to the stage of the loop running (ALL works as a wildcard to set them all at once, is overridden by more specific stage settings).\
MinSpeed/MaxSpeed - Minimum and Maximum animation speeds. Most animations allow between 1 and just below 1.5 in Slow, 1.5 to 2 for Fast loops and 1.5 to 2 for the Near Orgasm loop, but this allows overriding to anything you want. Note that non-zero values to ORGASM, POST_ORGASM or IDLE may result in functionally skipping some of the finishes or post-finish scenes (blur by too fast to see).\
SpeedEase - this is the transition pattern between min and max speeds. Available values listed in the comment of the config file, must exactly match (CASE SENSITIVE).\
LoopType - Pick Yoyo. Unless you know what you are doing.\
Min/Max FemaleOffset - This causes the female animation to become asynchronous to the male animation by these values in seconds. Recommend very small numbers, values over .25 seconds result in...odd animations.\
FemaleOffsetEase - Pattern of offset transitions, same as SpeedEase.

The config file is reloaded whenever Auto mode is engaged, so you can load an edit by just tapping CTRL-A (or whatever hotkey) twice. The animation speed changes require a loop change, so change positions or swap between slow->fast or any other loop change to trigger the new settings. New inclusions/exclusion take effect on the next animation selection event.
