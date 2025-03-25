module CounterTest

open Browser.Dom
open Browser.Types
open Fable.Jester
open Counter

Jest.describe("counter tests", fun () ->

    Jest.test("if setupCounter is called, counter starts at 0", fun () ->
        let element = document.createElement("div")
        setupCounter element
        Jest.expect(element.innerHTML).toBe("count is 0")
    )

    Jest.test("if element is clicked, counter increments", fun () ->
        let element = document.createElement("div")
        setupCounter element
        element.click()
        Jest.expect(element.innerHTML).toBe("count is 1")
    )

    Jest.test("if element is clicked twice, counter increments twice", fun () ->
        let element = document.createElement("div")
        setupCounter element
        element.click()
        element.click()
        Jest.expect(element.innerHTML).toBe("count is 2")
    )

    Jest.test("if setupCounter is called twice, counter is reset", fun () ->
        let element = document.createElement("div")
        setupCounter element
        element.click()
        setupCounter element
        Jest.expect(element.innerHTML).toBe("count is 0")
    )
)