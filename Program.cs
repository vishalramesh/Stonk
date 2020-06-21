using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

using System.Text.Json;
using System.Text.Json.Serialization;

class Window : Form {

    Graph graph;

    Button button = new Button();

    bool can_click_submit = false;
    bool draw_answers = false;
    bool mouse_down = false;
    bool mouse_move = false;
    bool display_next = false;

    const int EndYear = 2020;
    const int StartYear = 2016;

    Point blue1;
    Point blue2;
    bool draw_blue = false;

    Dictionary<int, Point> vertexPositions = new Dictionary<int, Point>();

    public Window(Graph graph) {
        Text = "Investment Game";
        this.graph = graph;
        DrawStartingLine();
        button.Location = new Point(220, 345);
        button.Text = "SHOW ME HOW I DID";
        button.Width = 170;
        button.Height = 40;
        button.BackColor = Color.LightGray;
        button.Font = new Font("Arial", 10);
        this.Controls.Add(button);
        button.Click += OnButtonClick;
    }

    public int x_to_year(int x) {
        if (x < 108) {
            return 2016;
        }
        return 2017 + (x - 108) / 96;
    }

    public double y_to_price(int y) {
        return graph.max_price * ((300 - y) / 210);
    }

    protected override void OnPaint(PaintEventArgs args) {
        Graphics g = args.Graphics;

        DrawAxes(g);

        DrawStockText(g);

        if (draw_blue) {
            g.DrawLine(new Pen(Color.Blue, 2), blue1, blue2);
        }

        if (can_click_submit) {
            button.BackColor = Color.IndianRed;
        } else {
            button.BackColor = Color.LightGray;
        }
        if (draw_answers) {
            button.Text = "TRY ANOTHER";
        }
        else {
            button.Text = "SHOW ME HOW I DID"; 
        }

        DrawExisting(g);

        if (draw_answers) {
            DrawWinning(g);
        }
    }

    public void OnButtonClick(Object sender, EventArgs e) {
        if (can_click_submit && draw_answers) {
            if (display_next) {
                GetNextReady();
            }
            display_next = true;
        }
        if (can_click_submit && !draw_answers) {
            draw_answers = true;
            Invalidate();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        if (mouse_down & !draw_answers) {

            foreach (int year in vertexPositions.Keys) {

                if (year == StartYear)
                    continue;
                if (Math.Abs(e.X - vertexPositions[year].X) <= 15 && Math.Abs(e.Y - vertexPositions[year].Y) <= 15 
                    && e.Y >= 80 && e.Y <= 300) {
                        vertexPositions[year] = new Point(vertexPositions[year].X, e.Y);
                        graph.changePrice(year, y_to_price(e.Y));
                        mouse_move = true;
                        break;
                    }
            }
        }

        if (!mouse_down && !draw_answers) {

            if (e.Y <= 300 && e.Y >= 90 && vertexPositions.ContainsKey(x_to_year(e.X) - 1) 
                && vertexPositions.ContainsKey(x_to_year(e.X)) == false) {
            
                for (int i = 0; i <= 4; ++i) {
                    if (108 + 96 * i < e.X && 108 + 96 * (i + 1) >= e.X) {
                        Point p = new Point();
                        p.X = 108 + 96 * (i + 1);
                        p.X = (i == 4) ? p.X - 48: p.X;
                        p.Y = e.Y;
                        // DrawLine
                        draw_blue = true;
                        blue1 = p;
                        Point p2 = new Point();
                        p2.X = 108 + 96 * i;
                        p2.Y = vertexPositions[x_to_year(p2.X - 20)].Y;
                        blue2 = p2;
                        Invalidate();
                    }
                }
            } else {
                draw_blue = false;
            }
        }

        if (!mouse_down && !draw_answers) {
            foreach (int year in vertexPositions.Keys) {
                if (Math.Abs(e.Y - vertexPositions[year].Y) <= 10 && x_to_year(e.X - 10) == year && x_to_year(e.X + 10) != year) {
                    // Console.WriteLine("sjjs");

                    break;
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
    }

    public void GetNextReady() {
        can_click_submit = false; 
        draw_answers = false;
        mouse_down = false;
        mouse_move = false;
        display_next = false;
        draw_blue = false;
        vertexPositions.Clear();
        graph.actual_vertices.Clear();
        graph.drawn.Clear();
        graph.next();
        DrawStartingLine();
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        mouse_down = false;
        if (mouse_move) {
            mouse_move = false;
            Invalidate();
        }
    }

    public void DrawStartingLine() {
        Point point2 = new Point();
        graph.addPoint(StartYear, graph.actual_vertices[StartYear]);
        point2.X = 60 + 48;
        point2.Y = 300 - (int)((graph.drawn[StartYear] / graph.max_price) * 210);
        vertexPositions.Add(StartYear, point2);
        
        Point point1 = new Point();
        graph.addPoint(2015, graph.actual_vertices[2015]);
        point1.X = 60;
        point1.Y = 300 - (int)((graph.drawn[2015] / graph.max_price) * 210);
        vertexPositions.Add(2015, point1);
        
    }

    public void DrawExisting(Graphics g) {
        Pen redPen = new Pen(Color.Red, 2);
        int year = 2015;
        while (vertexPositions.ContainsKey(year + 1)) {
            g.DrawLine(redPen, vertexPositions[year], vertexPositions[year + 1]);
            year += 1;
        }
    }

    public void DrawAxes(Graphics g) {

        Pen pen = new Pen(Color.FromArgb(255, 0, 0, 0), 2);
        Pen thinPen = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
        thinPen.DashCap = System.Drawing.Drawing2D.DashCap.Round;
        thinPen.Color = Color.LightGray;
        thinPen.DashPattern = new float[]{4.0F, 4.0F, 4.0F, 4.0F};

        Font drawFont = new Font("Arial", 8);
        SolidBrush blackBrush = new SolidBrush(Color.Black);

        g.DrawLine(pen, 60, 300, 540, 300); // X-axis
        int year = StartYear;
        for (int i = 60 + 48; i < 540; i += 96) { 
            g.DrawLine(pen, i, 295, i, 305); // Vertical dashed lines (short)
            g.DrawString(year.ToString(), drawFont, blackBrush, i - 15, 310); // Horizontal labels
            year += 1;
            g.DrawLine(thinPen, i, 300, i, 80); // Vertical dashed lines
        }
        g.DrawLine(thinPen, 540, 300, 540, 80); // Vertical dashed lines
        
        g.DrawString("June", drawFont, blackBrush, 60 - 15, 310);
        g.DrawString("June", drawFont, blackBrush, 540 - 15, 310);

        g.DrawLine(pen, 60, 300, 60, 80); // Y-axis

        // Horizontal dashed lines
        int x = 0;
        for (int i = 300; i >= 90; i -= (300-90)/(graph.max_price / graph.division)) {
            if (i != 300) {
                g.DrawLine(thinPen, 60, i, 540, i);
            }
            if (x != 0)
                g.DrawString((graph.division * x).ToString(), new Font("Arial", 9), blackBrush, 32, i - 8);
            x += 1;
        }
    }

    public void DrawWinning(Graphics g) {

        Pen greenPen = new Pen(Color.Green, 2);
        g.DrawLine(greenPen, vertexPositions[2015], vertexPositions[StartYear]);

        for (int i = StartYear; i <= EndYear; ++i) {
            Point pt1 = new Point();
            pt1.X = 108 + (i - StartYear) * 96;
            pt1.Y = 300 -  (int)((graph.actual_vertices[i] / graph.max_price) * 210);
            Point pt2 = new Point();
            pt2.X = 108 + (i + 1 - StartYear) * 96;
            if (i + 1 == 2021) {
                pt2.X -= 48;
            }
            pt2.Y = 300 - (int)((graph.actual_vertices[i + 1] / graph.max_price) * 210);
            g.DrawLine(greenPen, pt1, pt2);
        }
    }

    public void DrawStockText(Graphics g) {
        SolidBrush blackBrush = new SolidBrush(Color.Black);
        Font nameFont = new Font("Arial", 12);
        Font tickerFont = new Font("Arial", 9);
        g.DrawString(graph.name, nameFont, blackBrush, 400, 20);
        g.DrawString(graph.ticker, tickerFont, blackBrush, 400, 40);
    }

    [STAThread]
    static void Main() {
        Graph graph = new Graph();
        Form form = new Window(graph);
        form.Width = 600;
        form.Height = 450;
        form.BackColor = Color.White;
        form.KeyPreview = true;
        graph.changed += form.Invalidate;
        
        Application.Run(form);
    }
}

public class Year {

    public int year { get; set; }
    public double price { get; set; }

}

public class Share {

    public int id { get; set; }

    public string name { get; set; }

    public string ticker { get; set; }

    public int max_price { get; set; }

    public int division { get; set; }
        
    public List<Year> year_price { get; set; }
        
}

public class Share_Root {

    public List<Share> shares { get; set; }
}

delegate void Notify();

class Graph {
    public Notify changed;

    public string ticker { get; set; }

    public string name;
    public string Name { get; set; }

    public int max_price { get; set; }

    public int division { get; set; }


    public Dictionary<int, double> drawn {
        get {
            return drawn_vertices;
        }
    }

    public Dictionary<int, double> actual_vertices = new Dictionary<int, double>();
    
    Dictionary<int, double> drawn_vertices = new Dictionary<int, double>();

    int current_id = 0;
    int max_id = 6;

    Share_Root share_data;

    public Graph() {

        string jsonString = File.ReadAllText("shares.json");
        share_data = JsonSerializer.Deserialize<Share_Root>(jsonString);

        next();
    }

    public void addPoint(int year, double price) {
        drawn_vertices.Add(year, price);
        changed?.Invoke();
    }

    public void changePrice(int year, double price) {
        drawn_vertices[year] = price;
        changed?.Invoke();
    }

    public void next() {
        current_id += 1;
        foreach (var s in share_data.shares.ToArray()) {
            if (s.id == current_id) {
                division = s.division;
                max_price = s.max_price;
                ticker = s.ticker;
                name = s.name;
                foreach (var t in s.year_price.ToArray()) {
                    actual_vertices.Add(t.year, t.price);
                }
                break;
            }
        }        

    }

}