using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

class Window : Form {

    Graph graph;

    bool can_click_submit = false;
    bool draw_answers = false;
    bool mouse_down = false;
    bool mouse_move = false;
    bool display_next = false;

    Dictionary<int, Point> vertexPositions = new Dictionary<int, Point>();

    public Window(Graph graph) {
        Text = "Investment Game";
        this.graph = graph;
        Point point2 = new Point();
        point2.X = 60 + 48;
        point2.Y = 300 - (int)((graph.drawn[2016] / graph.max_price) * 210);
        vertexPositions.Add(2016, point2);
        Point point1 = new Point();
        point1.X = 60;
        point1.Y = 300;
        vertexPositions.Add(2015, point1);
    }

    public int x_to_year(int x) {
        return 2017 + (x - 108) / 96;
    }

    public double y_to_price(int y) {
        return graph.max_price * ((300 - y) / 210);
    }

    protected override void OnPaint(PaintEventArgs args) {
        Graphics g = args.Graphics;


        Pen pen = new Pen(Color.FromArgb(255, 0, 0, 0), 2);
        Pen thinPen = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
        thinPen.DashCap = System.Drawing.Drawing2D.DashCap.Round;
        thinPen.Color = Color.LightGray;
        thinPen.DashPattern = new float[]{4.0F, 4.0F, 4.0F, 4.0F};

        Font drawFont = new Font("Arial", 8);
        SolidBrush blackBrush = new SolidBrush(Color.Black);

        // Axis
        g.DrawLine(pen, 60, 300, 540, 300); // X-axis
        int yea = 2016;
        for (int i = 60 + 48; i < 540; i += 96) { // Horizontal labels
            g.DrawLine(pen, i, 295, i, 305);
            g.DrawString(yea.ToString(), drawFont, blackBrush, i - 15, 310);
            yea += 1;
            g.DrawLine(thinPen, i, 300, i, 80); // Vertical dashed lines
        }
        g.DrawLine(pen, 60, 300, 60, 80); // Y-axis
        g.DrawLine(thinPen, 540, 300, 540, 80);
        g.DrawString("June", drawFont, blackBrush, 60 - 15, 310);
        g.DrawString("June", drawFont, blackBrush, 540 - 15, 310);

        // Stock text
        g.DrawString(graph.name, new Font("Serif", 12), blackBrush, 400, 20);
        g.DrawString(graph.ticker, new Font("Serif", 9), blackBrush, 400, 40);

        // Button
        if (can_click_submit) {
            g.FillRectangle(new SolidBrush(Color.IndianRed), 220, 340, 180, 40);
        } else {
            g.FillRectangle(new SolidBrush(Color.LightGray), 220, 345, 170, 40);
        }
        if (draw_answers)
            g.DrawString("Try another".ToUpper(), new Font("Serif", 10), new SolidBrush(Color.White), 255, 355);
        else
            g.DrawString("Show me how I did".ToUpper(), new Font("Serif", 10), new SolidBrush(Color.White), 240, 355);   

        // Horizontal dashed lines
        int x = 0;
        for (int i = 300; i >= 90; i -= (300-90)/(graph.max_price / graph.division)) {
            if (i != 300) {
                g.DrawLine(thinPen, 60, i, 540, i);
            }
            if (x != 0)
                g.DrawString((graph.division * x).ToString(), new Font("Serif", 9), blackBrush, 32, i - 8);
            x += 1;
        }

        // Draw existing
        Pen redPen = new Pen(Color.Red, 2);
        // g.DrawLine(redPen, 60, 300, 60 + 48, 300 - (int)((graph.drawn[2016] / graph.max_price) * 210));
        int ye = 2015;
        while (vertexPositions.ContainsKey(ye + 1)) {
            g.DrawLine(redPen, vertexPositions[ye], vertexPositions[ye + 1]);
            ye += 1;
        }

        // Drawing winning as well
        Pen greenPen = new Pen(Color.Green, 2); 
        if (draw_answers) {
            for (int i = 2016; i <= 2020; ++i) {
                Point pt1 = new Point();
                pt1.X = 108 + (i - 2016)*96;
                pt1.Y = 300 -  (int)((graph.actual_vertices[i]/graph.max_price)*210);
                Point pt2 = new Point();
                pt2.X = 108 + (i + 1 - 2016)*96;
                if (i + 1 == 2021)
                    pt2.X -= 48;
                pt2.Y = 300 - (int)((graph.actual_vertices[i + 1]/graph.max_price)*210);
                g.DrawLine(greenPen, pt1, pt2);
            }
        }
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        if (mouse_down & !draw_answers) {
            foreach (int year in vertexPositions.Keys) {
                if (year == 2016)
                    continue;
                if (Math.Abs(e.X - vertexPositions[year].X) <= 15 && Math.Abs(e.Y - vertexPositions[year].Y) <= 15) {
                    if (e.Y >= 80 && e.Y <= 300) {
                        vertexPositions[year] = new Point(vertexPositions[year].X, e.Y);
                        mouse_move = true;
                        break;
                    }
                }
            }
        }
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        mouse_down = true;

        if (e.Y <= 300 && e.Y >= 90 && vertexPositions.ContainsKey(x_to_year(e.X) - 1) 
            && vertexPositions.ContainsKey(x_to_year(e.X)) == false) {
            
            for (int i = 0; i <= 4; ++i) {
                if (108 + 96 * i < e.X && 108 + 96 * (i + 1) >= e.X) {
                    Point p = new Point();
                    p.X = 108 + 96 * (i + 1);
                    p.X = (i == 4) ? p.X - 48: p.X;
                    can_click_submit = (i == 4);
                    p.Y = e.Y;
                    vertexPositions.Add(x_to_year(e.X), p);
                    graph.addPoint(x_to_year(e.X), y_to_price(e.Y));
                    Invalidate();
                }
            }
        }

        if (can_click_submit && !draw_answers)
            if (e.X >= 220 && e.X <= 220 + 170 && e.Y <= 385 && e.Y >= 345 ) {
                draw_answers = true;
                Invalidate();
            }
        if (can_click_submit && draw_answers) 
            if (display_next) {
                GetNextReady();
            }
            if (e.X >= 220 && e.X <= 220 + 170 && e.Y <= 385 && e.Y >= 345 ) {
                // Next
                display_next = true;
            }
    }

    public void GetNextReady() {
        can_click_submit = false; 
        draw_answers = false;
        mouse_down = false;
        mouse_move = false;
        display_next = false;
        vertexPositions = new Dictionary<int, Point>();
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        mouse_down = false;
        if (mouse_move) {
            mouse_move = false;
            Invalidate();
        }
    }

    [STAThread]
    static void Main() {
        Graph graph = new Graph();
        Form form = new Window(graph);
        form.Height = 450;
        form.Width = 600;
        form.BackColor = Color.White;
        form.KeyPreview = true;
        graph.changed += form.Invalidate;
        Application.Run(form);
    }   
}

delegate void Notify();

class Graph {
    public Notify changed;

    class Stock {
        string ticker;
        string name;
        int max_price;
        int division;
        Dictionary<int, double> actual_vertices = new Dictionary<int, double>();
    }

    Dictionary<string, string> stock = new Dictionary<string, string>();

    public string ticker {
        get {
            return stock["ticker"];
        }
    }

    public string name {
        get {
            return stock["name"];
        }
    }

    public int max_price {
        get {
            return 400;
        }
        set {
            max_price = value;
        }
    }

    public int division {
        get {
            return 100;
        }
        set {
            division = value;
        }
    }

    public Dictionary<int, double> drawn {
        get {
            return drawn_vertices;
        }
    }

    public Dictionary<int, double> actual_vertices = new Dictionary<int, double>();
    Dictionary<int, double> drawn_vertices = new Dictionary<int, double>();
    public Graph() {
        actual_vertices.Add(2015, 127.17);
        actual_vertices.Add(2016, 96.96);
        actual_vertices.Add(2017, 115.82);
        actual_vertices.Add(2018, 169.23);
        actual_vertices.Add(2019, 156.23);
        actual_vertices.Add(2020, 289.80);
        actual_vertices.Add(2021, 331.50);

        drawn_vertices.Add(2015, 127.17);
        drawn_vertices.Add(2016, 96.96);

        stock.Add("ticker", "NASDAQ: AAPL");
        stock.Add("name", "Apple Inc.");

        // Stock stock = new Stock();
    }

    public void addPoint(int year, double price) {
        drawn_vertices.Add(year, price);
        changed?.Invoke();
    }

    public void changePrice(int year, double price) {
        drawn_vertices[year] = price;
        changed?.Invoke();
    }

}