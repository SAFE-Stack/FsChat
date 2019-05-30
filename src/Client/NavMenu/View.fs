module NavMenu.View

open Browser.Dom
open Fable.Core.JsInterop
open Fable.React
open Props

open Router
open Channel.Types
open RemoteServer.Types
open Chat.Types

let menuItem htmlProp name topic isCurrent =
    button
      [ classList [ "btn", true; "fs-channel", true; "selected", isCurrent ]
        htmlProp ]
      [ h1 [] [str name]
        span [] [str topic]]

let menuItemChannel (ch: ChannelInfo) currentPage = 
    let targetRoute = Channel ch.Id
    let jump _ = document.location.hash <- toHash targetRoute
    menuItem (OnClick jump) ch.Name ch.Topic (targetRoute = currentPage)

let menuItemChannelJoin dispatch = 
    let join chid _ = chid |> Join |> dispatch
    fun (ch: ChannelInfo) ->
      menuItem (OnClick <| join ch.Id) ch.Name ch.Topic false

let menu (chatData: ChatState) currentPage dispatch =
    match chatData with
    | NotConnected ->
      [ div [] [str "not connected"] ]
    | Connecting _ ->
      [ div [] [str "connecting"] ]
    | Connected { serverData = { Me = me; NewChanName = newChanName; Channels = channels; ChannelList = channelList } } ->
      let opened, newChanName = newChanName |> function |Some text -> (true, text) |None -> (false, "")
      [ yield div
          [ ClassName "fs-user" ]
          [ UserAvatar.View.root me.ImageUrl
            h3 [Id "usernick"] [str me.Nick]
            span [Id "userstatus"] [ str me.Status]
            button
              [ Id "logout"; ClassName "btn"; Title "Logout"
                OnClick (fun _ -> document.location.href <- "/logoff") ]
              [ i [ ClassName "mdi mdi-logout-variant"] [] ]
           ]
        yield h2 []
          [ str "My Channels"
            button
              [ ClassName "btn"; Title "Create New"
                OnClick (fun _ -> (if opened then None else Some "") |> (SetNewChanName >> dispatch)) ]
              [ i [ classList [ "mdi", true; "mdi-close", opened; "mdi-plus", not opened ] ] []]
          ]
        yield input
          [ Type "text"
            classList ["fs-new-channel", true; "open", opened]
            Placeholder "Type the channel name here..."
            DefaultValue newChanName
            AutoFocus true
            OnChange (fun ev -> !!ev.target?value |> (Some >> SetNewChanName >> dispatch) )
            OnKeyPress (fun ev -> if !!ev.which = 13 || !!ev.keyCode = 13 then dispatch CreateJoin)
            ]

        for (_, ch) in channels |> Map.toSeq do
          yield menuItemChannel ch.Info currentPage

        yield h2 []
            [ str "All Channels"
              button
                [ ClassName "btn"; Title "Search" ]
                [ i [ ClassName "mdi mdi-magnify" ] []]
            ]
        for (chid, ch) in channelList |> Map.toSeq do
            if not(channels |> Map.containsKey chid) then
                yield menuItemChannelJoin dispatch ch
      ]
