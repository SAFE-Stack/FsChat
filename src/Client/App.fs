module App.View

open Elmish
open Elmish.Navigation
open Elmish.UrlParser
open Fable.Websockets.Elmish
open Fable.Core.JsInterop

open Types
open App.State
open Router
open Chat.Types

open Fable.React
open Props

importAll "./sass/app.scss"

let root model dispatch =

    let mainAreaView = function
        | Route.Overview -> [Overview.View.root]
        | Channel chan ->
            let toChannelMessage m = RemoteServer.Types.ChannelMsg (chan, m)

            match model.chat with
            | Connected { serverData = { Channels = channels }} when channels |> Map.containsKey chan ->

                Channel.View.root channels.[chan]
                  (toChannelMessage >> ApplicationMsg >> ChatDataMsg >> dispatch)

            | _ ->
                [div [] [str "bad channel route" ]]

    div
      [ ClassName "container" ]
      [ div
          [ ClassName "col-md-4 fs-menu" ]
          (NavMenu.View.menu model.chat model.currentPage (ApplicationMsg >> ChatDataMsg >> dispatch))
        div
          [ ClassName "col-xs-12 col-md-8 fs-chat" ]
          (mainAreaView model.currentPage) ]

open Elmish.React
open Elmish.Debug
open Elmish.HMR

// App
Program.mkProgram init update root
|> Program.toNavigable (parseHash Router.route) urlUpdate
#if DEBUG
|> Program.withDebugger
#endif
|> Program.withReactBatched "elmish-app"
|> Program.run
