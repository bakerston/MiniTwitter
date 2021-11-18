﻿open System
open Akka.FSharp
open System.IO
open System.Security.Cryptography
open Akka.Configuration



type registerMsg = {
    user_id: int
}

type operationMsg = {
    num_of_users: int
    operation_id: int
}

//type tweetIdMsg = {
//    user_id: int   
//}

////Local Client and Remote Server
let config =
    Configuration.parse
        @"akka {
            actor {
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                debug : {
                    receive : on
                    autoreceive : on
                    lifecycle : on
                    event-stream : on
                    unhandled : on
                }
            }
            remote.helios.tcp {
                hostname = ""localhost""
                port = 9001
            }
        }"

let system = System.create "RemoteClient" config
let RemoteServer = system.ActorSelection("akka.tcp://Project4@localhost:9002/user/RemoteServer")
let tweetIdServer = system.ActorSelection("akka.tcp://Project4@localhost:9002/user/tweetIdActor")

let CreateUsersActor =
    spawn system ("Actor" + "-Creator")
        (fun mailbox ->          
            let buildTime = Diagnostics.Stopwatch()
            buildTime.Start()
            let rec loop() = actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()             
                match box message with
                | :? int as msg ->
                    printfn "%i" msg
                | :? string as msg ->
                    printfn "%s" msg
                | :? registerMsg as msg ->
                    let user_dig = msg.user_id
                    let user_id = "user" + (user_dig |> string)
                    let msgToServer = "reg,"+user_id+",,,,," 
                    let resp = RemoteServer <! msgToServer
                    printfn "[Register]: User %i with user id of %s" user_dig user_id
                    printfn "[Response from Server]: %s" (resp |> string)
                    Threading.Thread.Sleep(10)
                | _ -> () 
                return! loop()
            }
            loop()
        )

let GenerateMessageActor =
    spawn system ("Actor" + "-Generator")
        (fun mailbox ->          
            let buildTime = Diagnostics.Stopwatch()
            buildTime.Start()
            let rec loop() = actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()             
                match box message with
                | :? int as msg ->
                    printfn "%i" msg
                | :? string as msg ->
                    //let handler = system.ActorSelection("akka.tcp://Project4@localhost:9002/user/Actor-MsgHandler")
                    printfn "%s" msg
                | :? operationMsg as msg ->
                    let operation_id = msg.operation_id
                    let num_of_user = msg.num_of_users
                    let mutable operation_type = ""
                    let mutable msgToServer = ""
                    match operation_id with 
                    | 1 ->
                        operation_type <- "sub"
                        let mutable user1 = 0
                        let mutable user2 = 0
                        let mutable user1_id = ""
                        let mutable user2_id = ""
                        user1 <- Random().Next(0, num_of_user)
                        user2 <- Random().Next(0, num_of_user)
                        user1_id <- "user" + (user1 |> string)
                        user2_id <- "user" + (user2 |> string)
                        while user2 = user1 do
                            user2 <- Random().Next(0, num_of_user)
                        msgToServer <- "sub,"+user1_id+",,,"+ user2_id + ",,"
                        let handler = system.ActorSelection("akka.tcp://Project4@localhost:9002/user/Actor-MsgHandler")
                        handler <! msgToServer

                    | 2 ->
                        operation_type <- "send"
                        let mutable user1 = 0
                        user1 <- Random().Next(0, num_of_user)
                        let user1_id = "user" + (user1 |> string)
                        let value = Random().Next(num_of_user, 5*num_of_user)
                        let tweet_content = "(" + (user1|>string) + "th user's tweet with a number of" + (value|>string) + ")"
                        let mutable stringTag = ""
                        let mutable stringMen = ""
                        let mutable numTag = 0
                        let mutable numMen = 0
                        let mutable curTag = 0
                        let mutable curMen = 0
                        let maxTag = 3
                        let maxMen = 3
                        numTag <- Random().Next(0, maxTag + 1) 
                        numMen <- Random().Next(0, maxMen + 1)
                        if numTag >= 1 then
                            curTag <- Random().Next(0, num_of_user)
                            stringTag <- "Tag" + (curTag |> string)
                            if numTag > 1 then
                                for k = 1 to numTag do
                                    curTag <- Random().Next(0, num_of_user)
                                    stringTag <- stringTag + "#Tag" + (curTag |> string)
                        if numMen >= 1 then
                            curMen <- Random().Next(0, num_of_user)
                            stringMen <- "user" + (curMen |> string)
                            if numMen > 1 then
                                for k = 1 to numMen do
                                    curMen <- Random().Next(0, num_of_user)
                                    stringMen <- stringMen + "@user" + (curMen |> string)
                        msgToServer <- "send,"+user1_id+","+tweet_content+",,,"+stringTag+","+stringMen
                        let handler = system.ActorSelection("akka.tcp://Project4@localhost:9002/user/Actor-MsgHandler")
                        handler <! msgToServer
                    | 3 -> 
                        operation_type <- "retw"
                        let user1 = Random().Next(0, num_of_user)
                        let user1_id = "user" + (user1 |> string)
                        let msgToId = user1_id
                        let resp_1 = tweetIdServer <! msgToId
                        let tweet_id = resp_1 |> string
                        Threading.Thread.Sleep(10)
                        msgToServer <- "retw,"+user1_id+",,"+tweet_id+",,,"
                        let handler = system.ActorSelection("akka.tcp://Project4@localhost:9002/user/Actor-MsgHandler")
                        handler <! msgToServer
                    | 4 ->
                        operation_type <- "query"
                        let user1 = Random().Next(0, num_of_user)
                        let user1_id = "user" + (user1 |> string)
                        msgToServer <- "query,"+user1_id+",,,,,"
                        let handler = system.ActorSelection("akka.tcp://Project4@localhost:9002/user/Actor-MsgHandler")
                        handler <! msgToServer

                    | 5 ->
                        operation_type <- "quert"
                        let tag_id = Random().Next(0, num_of_user)
                        let tag = (tag_id |> string)
                        msgToServer <- "quert,,,,,"+tag+","
                        let handler = system.ActorSelection("akka.tcp://Project4@localhost:9002/user/Actor-MsgHandler")
                        handler <! msgToServer
                    | 6 ->
                        operation_type <- "querm"
                        let men_id = Random().Next(0, num_of_user)
                        let men = "user" + (men_id |> string)
                        msgToServer <- "querm,,,,,,"+men
                        let handler = system.ActorSelection("akka.tcp://Project4@localhost:9002/user/Actor-MsgHandler")
                        handler <! msgToServer
                    | _ ->
                        printfn "400 Bad Request!"
                | _ -> () 
                return! loop()
            }
            loop()
        )
 

[<EntryPoint>]
let main argv =
    printf "Input the number of users: "
    let N = Console.ReadLine() |> int

    // Create N users with user_id
    for i = 0 to N - 1 do
        let createUserMsg = {user_id = i}
        let resp = CreateUsersActor <! createUserMsg
        printfn "[Register]: User %i" i 

    // Create Zipf Distribution of subscribers.
    let mutable increment = 0
    let mutable user1_id = ""
    let mutable user2_id = ""
    let mutable msgToServer = ""
    for i = 0 to N - 1 do
        increment <- i + 1
        for j in 0..increment..N - 1 do
            if not (i=j) then
                user1_id <- "user" + (i |> string)
                user2_id <- "user" + (j |> string)
                msgToServer <- "sub," + user1_id + ",,," + user2_id + ",,"
                printfn "[Client Subscribe]: User %i to User %i" i j
                RemoteServer <! msgToServer
                
           

    // Users posts tweets
    let mutable numPost = 0
    let maxPost = (int)(N/5)
    let mutable numTag = 0
    let mutable curTag = 0
    let maxTag = 3
    let mutable numMen = 0
    let mutable curMen = 0
    let maxMen = 3
    let mutable stringTag = ""
    let mutable stringMen = ""
    let mutable tweetContent = ""

    for i = 0 to N - 1 do
        numPost <- maxPost - (int)(i/10)
        user1_id <- "user" + (i|>string)
        for j = 0 to numPost - 1 do
            stringTag <- ""
            stringMen <- ""
            numTag <- Random().Next(0, maxTag + 1) 
            numMen <- Random().Next(0, maxMen + 1)
            if numTag >= 1 then
                curTag <- Random().Next(0, N)
                stringTag <- "Tag" + (curTag |> string)
                if numTag > 1 then
                    for k = 1 to numTag do
                        curTag <- Random().Next(0, N)
                        stringTag <- stringTag + "#Tag" + (curTag |> string)
            if numMen >= 1 then
                curMen <- Random().Next(0, N)
                stringMen <- "user" + (curMen |> string)
                if numMen > 1 then
                    for k = 1 to numMen do
                        curMen <- Random().Next(0, N)
                        stringMen <- stringMen + "@user" + (curMen |> string)
            tweetContent <- "(" + (i|>string) + "th user " + (j|>string) + "th tweets)"
            msgToServer <- "send,"+user1_id+","+tweetContent+",,,"+stringTag+","+stringMen
            printfn "[Client Post]: %i th user post %i th tweets!" i j
            RemoteServer <! msgToServer            
            //printfn "[Response from Server]: %s" (resp |> string)

    // Users retweets
    let mutable msgToId = ""
    let mutable resp_1 = ""
    let mutable tweet_id = ""
    let tweetIdActor = system.ActorSelection("akka.tcp://Project4@localhost:9001/user/tweetIdActor")
    for i = 0 to N - 1 do
        user1_id <- "user" + (i|>string)
        msgToId <- user1_id
        tweetIdActor <! msgToId
        printfn "[Client Retweet]: %i th user retweets!" i
        //let resp = msgToServer/Hnader <! msgTold
        //tweet_id <- resp_1 |> string
        //msgToServer <- "retw,"+user1_id+",,"+tweet_id+",,,"

    // Randomly apply 100 operations, except the registeration.
    let mutable op_id = 0
    for k = 0 to 100 do
        op_id <- Random().Next(1, 7)
        let msgToGenerator = {num_of_users = N; operation_id = op_id}
        printfn "[Client Random Operation]: operation id = %i" op_id
        GenerateMessageActor <! msgToGenerator

 

    0 // return an integer exit code