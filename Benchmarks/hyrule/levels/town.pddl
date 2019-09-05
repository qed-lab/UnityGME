(define (problem O1)
	(:domain HYRULE)
	(:objects 
		FLOAT INT 
		;; ---- Prefabs
		player wizard fortuneteller 	;; types of characters
		cave sand woods town 			;; types of rooms
		house shop lounge 				;; types of buildings
		ladder book sword key door
		
		;; ---- Character Objects ----
		arthur merlin edgar
		
		;; ---- Items
		g1 g2 spellbook merlinbook trolldoorkey
		
		;; ---- Door and Entrance Objects
		barentrance barexit
		fortuneentrance fortuneexit
		shopentrance shopexit
		jailentrance jailexit
		churchentrance churchexit
		mansionentrance mansionexit
		hermithouseentrance hermithouseexit
		trolldoor
		
		
		;; ---- Location Objects ----
		cliffside 
		docks bar 
		townarch fortuneroom 
		townsquare shop jail church alley
		towncliff mansion
		villageroad
		trollbridge
		hermitcliff hermithouse
		castle
		
	)
	(:init
		;; ---- Characters ----
		(character arthur)		(prefab arthur player)			(player arthur) 
		(character merlin)		(prefab merlin wizard)			(npc merlin)
		(character edgar)		(prefab edgar fortuneteller)	(npc fortuneteller)
		
		;; ---- Items ----
		(book spellbook)	(prefab spellbook book)		(thing spellbook) 
		(book merlinbook)	(prefab merlinbook book)	(thing merlinbook)
		(sword g1)			(prefab g1 sword)			(thing g1)	
		(sword g2)			(prefab g2 sword)			(thing g2)     
		
		(key trolldoorkey)	(prefab trolldoorkey key)	(thing trolldoorkey)
		(door trolldoor)	(prefab trolldoor door)		(thing trolldoor)
		(opens trolldoorkey trolldoor)
		
		;; ---- World Map ----
		(location cliffside)    		(prefab cliffside cave)
		(location docks) 				(prefab docks sand)
		(location bar)					(prefab bar cave)
		(location townarch)				(prefab townarch town)
		(location fortuneroom)			(prefab fortuneroom cave)
		(location townsquare)			(prefab townsquare town)
		(location shop)					(prefab shop cave)
		(location jail)					(prefab jail sand)
		(location church)				(prefab church woods)
		(location towncliff)			(prefab towncliff sand)
		(location alley)				(prefab alley town)
		(location mansion)				(prefab mansion town)
		(location villageroad)			(prefab villageroad woods)
		(location trollbridge)			(prefab trollbridge sand)
		(location hermitcliff)			(prefab hermitcliff woods)
		(location hermithouse)			(prefab hermithouse town)
		
		(entrance barentrance)			(prefab barentrance ladder)
		(entrance barexit)				(prefab barexit ladder)
		
		(entrance fortuneentrance)		(prefab fortuneentrance ladder)
		(entrance fortuneexit)			(prefab fortuneexit ladder)
		
		(entrance shopentrance)			(prefab shopentrance ladder)
		(entrance shopexit)				(prefab shopexit ladder)
		
		(entrance jailentrance)			(prefab jailentrance ladder)
		(entrance jailexit)				(prefab jailexit ladder)
		
		(entrance churchentrance)		(prefab churchentrance ladder)
		(entrance churchexit)			(prefab churchexit ladder)
		
		(entrance mansionentrance)		(prefab mansionentrance ladder)
		(entrance mansionexit)			(prefab mansionexit ladder)
		
		(entrance hermithouseentrance)	(prefab hermithouseentrance ladder)
		(entrance hermithouseexit)		(prefab hermithouseexit ladder)
	
	
		;; The cliffside connects to the docks and to the village road
		(connected cliffside docks)		(nodoor cliffside docks)
		(connected docks cliffside)		(nodoor docks cliffside)
		
		(connected cliffside villageroad)	(nodoor cliffside villageroad)
		(connected villageroad cliffside)	(nodoor villageroad cliffside)
		
		;; The village road connects to the troll bridge
		(connected villageroad trollbridge)	(nodoor villageroad trollbridge)
		(connected trollbridge villageroad) (nodoor trollbridge villageroad)
		
		;; The troll bridge connects to the hermit cliff, via a locked door
		(connected trollbridge hermitcliff) (between trolldoor trollbridge hermitcliff)
		(connected hermitcliff trollbridge)	(between trolldoor hermitcliff trollbridge)	(locked trolldoor)
		
		;; The hermit cliff contains the hermit's house
		(at hermithouseentrance hermitcliff)	(leadsto hermithouseentrance hermithouse)
		(at hermithouseexit hermithouse)		(leadsto hermithouseexit hermitcliff)
		
		;; The docks contains the bar and connects to the town archway
		(at barentrance docks)			(leadsto barentrance bar)
		(at barexit bar)				(leadsto barexit docks)
		
		(connected docks townarch)		(nodoor docks townarch)
		(connected townarch docks)		(nodoor townarch docks)
		
		;; The town archway contains the fortuneteller's room, and connects to the town square
		(at fortuneentrance townarch)	(leadsto fortuneentrance fortuneroom)
		(at fortuneexit fortuneroom)	(leadsto fortuneexit townarch)
		
		(connected townarch townsquare) (nodoor townarch townsquare)
		(connected townsquare townarch) (nodoor townsquare townarch)
		
		;; The town square contains...
		;; ...the shop,
		(at shopentrance townsquare)	(leadsto shopentrance shop)
		(at shopexit shop)				(leadsto shopexit townsquare)
		
		;; ...the jail,
		(at jailentrance townsquare)	(leadsto jailentrance jail)
		(at jailexit jail)				(leadsto jailexit townsquare)
		
		;; ...the church,
		(at churchentrance townsquare)	(leadsto churchentrance church)
		(at churchexit church)			(leadsto churchexit townsquare)
		
		;; ...and connects to the alley and town cliffside.
		(connected townsquare alley)		(nodoor townsquare alley)
		(connected alley townsquare)		(nodoor alley townsquare)
		
		(connected townsquare towncliff)	(nodoor townsquare towncliff)
		(connected towncliff townsquare)	(nodoor towncliff townsquare)
		
		
		;; The town cliffside contains the governor's mansion
		(at mansionentrance towncliff)	(leadsto mansionentrance mansion)
		(at mansionexit mansion)		(leadsto mansionexit towncliff)
		
		;; ---- Initial Configuration ----
		(at trolldoorkey docks)		
		(at merlin townarch)			(has merlin merlinbook)				(asleep merlin)
		(at arthur cliffside)
		(at edgar bar)
		(at spellbook docks)
		(at g1 townarch)				(enchanted g1)
		;;(at g2 castle)
	)
	(:goal 
		;;(and
			(has arthur g1)
			;;(has arthur g2)
		;;)
	)
)