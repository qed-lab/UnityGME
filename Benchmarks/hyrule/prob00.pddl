(define (problem TUTORIAL)
   (:domain HYRULE)
   (:objects
        ;; Dramatis Personae
        arthur mel - character

        ;; Locations
        basement storage bar - location

        ;; Entrances
        basemententrance basementexit - entrance

        ;; Items
        basementexitkey bucket - item

        ;; Doors
        basementgate - door

        ;; Character Prefabs
        player wizard - prefab

        ;; Location Prefabs
        brickhouse woodenhouse - prefab

        ;; Item Prefabs
        woodendoor gate - prefab
        key - prefab
        pailofwater - prefab
   )
   (:init
        ;; Player Character
        (player arthur)

        ;; ---- Characters ----
        (prefab arthur player)
        (prefab mel wizard)

        ;; ---- Items ----
        (prefab bucket pailofwater)

        ;; ---- Locations ----
        (prefab basement brickhouse)
        (prefab storage brickhouse)
        (prefab bar woodenhouse)

        ;; ---- Keys and Locks ----
        (prefab basementexitkey key)
        (prefab basementgate gate)
        (unlocks basementexitkey basementexit)

        ;; ---- Entrances ----
        (prefab basemententrance woodendoor)
        (prefab basementexit woodendoor)


        ;; ---- World Map ----

        ;; The storage room connects to the basement.
        (connected storage basement)    (doorbetween basementgate storage basement)
        (connected basement storage)    (doorbetween basementgate basement storage)

        ;; The basement contains the basementexit (toward the bar)
        (at basementexit basement)      (leadsto basementexit bar)
        (at basemententrance bar)       (leadsto basemententrance basement)
        (closed basementexit)
        (locked basementexit)

        ;; ---- Initial Configuration ----

        (at mel storage)        (at bucket storage)
                                (wants-item mel bucket)
                                (willing-to-give-item mel basementexitkey)


        (at arthur storage)     (at basementexitkey basement)


    )
    (:goal
        (and
			;; Equip Quest
			;; (has ian knightsword) -> added dynamically
			;; (has ian knightshield) -> added dynamically

			;; Pilgrimage Quest
			;; (has alli tastycupcake) -> added dynamically

			;; Wisdom Quest
			;; (has james coin) -> added dynamically
			;; (has james humanskull) -> added dynamically
			;; (has james candle) -> added dynamically

			;; Fetch Quest
			;; (has giovanna hairtonic) -> added dynamically

			;; Love Quest
			;; (has jordan loveletter) -> added dynamically
			;; (has jordan lovecontract) -> added dynamically

			;; Other
			;; (has arthur ash) -> added dynamically

			;; The Win Condition!
			   (game-has-been-won)
        )
    )
)
